# API Store V1 — contrat et utilisation

Validation locale du 9 novembre 2026 : **316 tests unitaires et 187 tests d'intégration réussis**,
MySQL 8.4 LTS, migrations cohérentes pour les deux contextes, formatage et audit NuGet sans
vulnérabilité signalée. Images API et ClamAV construites en AMD64 et ARM64 ; sondes du vrai
scanner testées avec un texte/signature inertes. L'image API démarre avec un compte non-root,
répond à `/health/live` et refuse `/health/ready` lorsqu'aucune base n'est accessible.

Le workflow de publication des images reste manuel, après validation de la CI.
Il ne déploie pas automatiquement l'API sur le Raspberry Pi.
Avant lancement réel : configurer les clés publiques, le proxy et les volumes du Pi,
tester une sauvegarde/restauration et vérifier de vrais paquets signés sur Windows/Android.

Ce lot termine le backend de la première tranche Store Windows/Android : catalogue invité,
versions, fichiers, publication et téléchargement. Le client MAUI et l'installation native
restent à développer. L'Auth actuel du Manager est conservé ; il ne constitue pas encore
le service OIDC/passkeys requis par le futur Vault.

## Routes publiques

| Méthode et chemin | Résultat |
|---|---|
| `GET /api/store/v1/apps` | Page bornée : `Items`, `Page`, `PageSize`, `TotalCount`, `TotalPages` |
| `GET /api/store/v1/apps/search?q=notes` | Même contrat, recherche dans nom et description |
| `GET /api/store/v1/apps/{slug}` | Fiche publique et lien d'icône |
| `GET /api/store/v1/categories` | Catégories ayant au moins une application publique |
| `GET /api/store/v1/apps/{slug}/icon` | Icône de l'application publiée, ou 404 |
| `GET /api/store/v1/apps/{slug}/releases/latest` | Fichier compatible de la version installable la plus élevée |
| `GET /api/store/v1/artifacts/{id}/download` | Flux binaire, Range/If-Range, ETag/If-None-Match |

Filtres du catalogue : `q`, `categoryId`, `page` (à partir de 1), `pageSize` (1–100),
`platform`, `arch`, `osVersion`, `channel=stable`. `platform` vaut `windows` ou `android`.
Pour `latest`, `platform` et `arch` sont obligatoires. `osVersion` représente la version
Windows (`10.0.19041.0`) ou le **niveau API Android** (`26`, pas « Android 8 »).
Une architecture neutre Windows ou universelle Android peut servir de solution compatible.
Une valeur inconnue ou incohérente retourne 400 ; aucun résultat compatible retourne 404.

Une application doit être visible et avoir une release `Published` avec un fichier validé,
signé par une clé de publication encore autorisée. Les brouillons et releases `Withdrawn`
sont absents de toutes les routes publiques, même avec un identifiant de fichier connu.
Les réponses publiques n'exposent aucun chemin disque. Les contrats se trouvent dans
`ArzenalStoreService/Contracts/Store` et sont décrits dans l'OpenAPI de l'API.
Ils conservent la casse PascalCase des réponses historiques du projet.

## Administration et publication

Toutes les routes sous `/api/store/v1/admin` exigent la même identité Admin/Manager
que les routes WPF historiques. L'auteur de l'audit provient du jeton côté serveur.

1. `POST /apps` avec `{ "Name": "Notes", "Slug": "notes", "IsVisible": true }`.
2. `POST /apps/{appId}/releases` avec `{ "Version": "1.0.0", "Channel": "stable", "Changelog": "Première version" }`.
3. Signer le fichier sur le PC de l'éditeur puis envoyer son contenu brut avec
   `POST /releases/{releaseId}/artifacts`, `Content-Type: application/octet-stream`.
4. `POST /releases/{releaseId}/publish` publie explicitement la release validée.
5. `POST /releases/{releaseId}/withdraw` la retire sans supprimer l'historique ou les fichiers.

Métadonnées de l'upload dans la query string : `platform`, `architecture`, `packageType`,
`packageId`, `packageVersion`, `minimumOsVersion` et, pour Android, `androidVersionCode`.
Seuls `windows/msix` et `android/apk` sont acceptés. En-têtes obligatoires :
`X-Publisher-Key-Id` et `X-Artifact-Signature` (RSA-PSS/SHA-256, Base64).

Autres routes : `PUT /apps/{id}` pour configurer une fiche existante ou la masquer,
`GET /apps/{id}/releases`, `GET /releases/{id}`, `GET /apps/{id}/audit` (500 dernières entrées),
`DELETE /releases/{releaseId}/artifacts/{artifactId}` pour supprimer un fichier brouillon.
Le slug ne peut plus changer après une première publication. Les suppressions historiques
d'applications/fichiers sont refusées pour une application passée au Store V1.

Les validations sont synchrones : `Draft → Uploaded → Published → Withdrawn`.
Une validation refusée laisse le brouillon inchangé. Une variante existante n'est jamais
écrasée ; un brouillon doit d'abord supprimer son fichier avant remplacement.
Après publication, tout changement de binaire requiert une nouvelle release.
L'identité du paquet doit rester la même et la version installable doit augmenter.
Les verrous MySQL sérialisent uploads/publications de la même application.

## Confiance : distinction importante

La signature RSA-PSS vérifie **la provenance des octets du fichier**, avec une clé publique
autorisée dans `StoreDistribution:PublisherPublicKeys`. Le serveur n'a jamais besoin de
la clé privée de l'éditeur. SHA-256 seul ne prouve pas cette provenance.

Cette preuve de distribution **ne remplace pas la signature native MSIX/APK** :

- Le serveur contrôle la structure ZIP, les tailles, les doublons et chemins dangereux.
- Pour MSIX, il vérifie l'identité/version/architecture du manifeste, le minimum Windows
  et la présence de la blockmap/signature. Il ne prétend pas vérifier Authenticode.
- Pour APK, les métadonnées sont déclarées par l'éditeur autorisé ; le serveur ne décode
  pas le manifeste Android binaire et ne prétend pas vérifier les schémas APK v2/v3.
- Le futur client doit épingler les clés publiques de distribution via un canal de confiance,
  vérifier les octets/signatures reçus et utiliser les vérifications natives Windows/Android
  pour contrôler identité, éditeur, signature et compatibilité **avant installation**.
  Ne pas afficher un bouton Installer tant que ce parcours n'est pas fait et testé.

L'API est utilisable pour développer le catalogue et les téléchargements ; la validation
complète d'installation sur de vrais paquets Windows/Android reste une condition de
livraison du Store, pas une propriété prouvée par les archives inertes des tests serveur.

## Préparer la clé de publication sans mot de passe supplémentaire

Sur le PC développeur Windows, PowerShell **7.5 ou ultérieur** (.NET 9+) :

```powershell
$publisher = ./scripts/New-StorePublisher.ps1
$publisher.CertificateThumbprint
dotnet user-secrets set --project ArzenalStoreApi 'StoreDistribution:PublisherPublicKeys:main' $publisher.PublicKeyPem
$proof = ./scripts/Sign-StoreArtifact.ps1 -Path C:/packages/notes.msix -CertificateThumbprint $publisher.CertificateThumbprint
```

Le script crée une clé RSA 3072 non exportable dans le magasin de certificats de l'utilisateur
Windows. Conserver l'empreinte. Elle est différente d'un certificat de signature MSIX.
Prévoir une seconde clé autorisée/une stratégie de rotation avant de remplacer ou perdre ce PC.
La clé publique seule peut être transmise au serveur. Dans `deploy/.env`, utiliser la valeur PEM
sur une ligne pour `STORE_PUBLISHER_PUBLIC_KEY`, associée à l'identifiant `main`.
Sans clé publique configurée, les uploads V1 sont refusés. Ne jamais envoyer de clé privée à l'API.

Une session HTTP Manager valide peut ensuite publier, par exemple :

```powershell
# $session provient du login existant ; ne pas écrire les cookies/tokens dans les sources.
$headers = @{ 'X-Publisher-Key-Id' = $proof.PublisherKeyId; 'X-Artifact-Signature' = $proof.Signature }
$uri = "$api/api/store/v1/admin/releases/$releaseId/artifacts?platform=windows&architecture=x64&packageType=msix&packageId=Arzenal.Notes&packageVersion=1.0.0.0&minimumOsVersion=10.0.19041.0"
Invoke-RestMethod -Method Post -Uri $uri -WebSession $session -Headers $headers -ContentType application/octet-stream -InFile C:/packages/notes.msix
Invoke-RestMethod -Method Post -Uri "$api/api/store/v1/admin/releases/$releaseId/publish" -WebSession $session
```

## Migrations et compatibilité

`AddReleases` puis `AddStoreDistribution` sont nécessaires. Les anciennes applications
restent privées (`Slug = NULL`) tant qu'une fiche Store n'est pas configurée. Les anciens
artefacts sont rattachés à des brouillons mais conservent leur chemin et ne sont ni validés
ni publiés automatiquement. Les réimporter via V1 avant publication.

Les routes CRUD et uploads historiques du Manager continuent de fonctionner pour les apps
non migrées. Les anciennes routes de téléchargement/icône sont désormais Manager uniquement ;
les clients publics doivent utiliser `/api/store/v1`.
La base lie un artefact à une release de **la même application** par une clé étrangère composée.

## Vérification

```powershell
dotnet test ArzenalStoreApi.TestUnitaire/Arzenal.Store.Api.TestUnitaire.csproj
dotnet test Arzenal-Store-TestApi/Arzenal.Store.Api.TestIntegration.csproj
```

Les tests utilisent des clés éphémères et des archives inertes, jamais les clés de publication.
Ils couvrent migration/retour arrière, données historiques, filtres, anonymat, brouillons,
retrait, signatures de distribution, hash, Range/ETag, quota réel, concurrence et immutabilité.
Méthode pour les évolutions suivantes : test du comportement attendu, échec constaté,
implémentation, puis tests de régression.

Le scanner refuse les réponses incomplètes ou autres que `stream: OK`, même lorsqu'elles
arrivent en plusieurs fragments TCP. `MalwareScan:TimeoutSeconds` borne l'analyse à 60 secondes
par défaut (1–300). Une panne ou un délai expiré retourne 503, un fichier détecté retourne 400 ;
aucun fichier rejeté ne devient public. Les réponses d'erreur utilisent `application/problem+json`.
