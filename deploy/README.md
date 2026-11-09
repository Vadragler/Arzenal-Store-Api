# Déploiement Docker cloisonné

Cette composition remplace le principe d'un réseau Docker unique :

- `arzenal_edge` : le reverse proxy et l'API uniquement ;
- `arzenal_database` : l'API et MySQL uniquement ; ce réseau est `internal`, sans port publié ;
- `arzenal_malware_scan` : l'API et ClamAV uniquement ; ce réseau est aussi `internal` ;
- `arzenal_scan_updates` : ClamAV uniquement, sortie Internet pour mettre à jour ses signatures ; aucun port publié ;
- MySQL et ClamAV n'ont ni port publié vers l'hôte, ni réseau Docker commun : ils ne peuvent donc pas communiquer entre eux.

Le reverse proxy existant doit quitter le réseau partagé historique puis rejoindre **uniquement** `arzenal_edge`. Configure sa cible vers `http://arzenal-api:8080`. Son interface d'administration ne doit pas être publiée sur Internet ; limite-la à l'hôte, à un VPN ou à un réseau d'administration distinct.

Les outils de sécurité (ZAP, OpenVAS) ne font pas partie du déploiement applicatif permanent : lance-les dans une composition et un réseau séparés, ponctuellement. Ils ne doivent jamais partager `arzenal_database` ni `arzenal_malware_scan`.

## Mise en place

Construction locale depuis la racine du dépôt (le deuxième chemin cible le dossier contenant `Arzenal.Dto.csproj`) :

```powershell
docker build -f deploy/Dockerfile --build-context shared-dtos=../Arzenal-Store-Shared-Dtos/Arzenal-Store-Shared-Dtos -t arzenal-store-api:1.0.0 .
```

Le Dockerfile compile .NET 10 pour l'architecture cible et exécute l'API avec l'utilisateur
non privilégié `app`. Le contexte ignore les secrets et configurations locales.
Sur un Pi ARM64, utiliser une image ARM64 ; le système d'exploitation du Pi doit lui aussi être 64 bits.

Le service ClamAV utilise `deploy/clamav.Dockerfile`, fondé sur les paquets de sécurité Debian 12
disponibles pour AMD64/ARM64. `docker compose ... build clamav` le construit localement sur le Pi.
Le volume `clamav_data` conserve les signatures entre redémarrages. Le premier démarrage peut
prendre plusieurs minutes et nécessite l'accès sortant au CDN ClamAV pour FreshClam.
L'API attend le healthcheck ClamAV. Prévoir et mesurer la RAM disponible sur le Pi ; éviter
d'y lancer en même temps des scanners supplémentaires ou les tests d'intégration.

La cible MySQL est désormais **8.4 LTS**, 8.0 étant en fin de vie
([Oracle](https://dev.mysql.com/doc/relnotes/mysql/8.0/en/)). La matrice ClamAV et les correctifs
de distribution doivent rester surveillés ([ClamAV](https://docs.clamav.net/faq/faq-eol.html)).
Le build refuse ClamAV antérieur à 1.4.6 ; le choix Debian 12 suit les versions corrigées
du [suivi sécurité Debian](https://security-tracker.debian.org/tracker/source-package/clamav).
`scripts/Test-ClamavImage.ps1` démarre la configuration réelle avec une minuscule base de
signatures de test et un fichier texte inerte. Il vérifie le healthcheck et la détection, sans
virus réel et sans télécharger les définitions complètes. Le workflow manuel l'exécute avant publication.
Les images Debian doivent être reconstruites régulièrement pour recevoir les correctifs APT.
Avant une production existante en MySQL 8.0, sauvegarder et tester une migration vers un nouveau
volume 8.4 : ne pas changer la version sur l'ancien volume sans procédure de mise à niveau vérifiée.

Le workflow **Publish API image (manual)** peut être lancé dans GitHub Actions une fois le code
poussé : il exécute d'abord la CI, puis publie les variantes AMD64/ARM64 dans GHCR sous
`ghcr.io/<propriétaire>/<dépôt>:sha-<commit>`. Il ne déploie rien sur le Pi. Il reste manuel pour
éviter de consommer des minutes et publier des images à chaque petit changement.
`GITHUB_TOKEN` publie l'image ; `SHARED_DTOS_READ_TOKEN` conserve son accès au dépôt DTO uniquement.
Vérifier la visibilité du package et l'accès en lecture du Pi, puis préférer un digest `@sha256:...`
dans `ARZENAL_API_IMAGE`. Ne pas déployer un tag `latest` non contrôlé.
Le même workflow publie `ghcr.io/<propriétaire>/<dépôt>-clamav:sha-<commit>` ; renseigner
`CLAMAV_IMAGE` avec cette image pour éviter une construction sur le Pi.

1. Construire et publier l'image API sous un tag immuable, par exemple `arzenal-store-api:1.0.0`.
2. Copier `.env.example` vers `.env` et remplacer toutes les valeurs.
3. Créer les réseaux et valider la syntaxe :

```powershell
docker compose --env-file deploy/.env -f deploy/compose.yaml config
docker compose --env-file deploy/.env -f deploy/compose.yaml up -d
```

L'API applique les migrations au démarrage uniquement parce que `Database__ApplyMigrationsOnStartup=true` dans cette composition et qu'elle ne contient qu'une instance API. Pour plusieurs réplicas, lancer les migrations une seule fois dans la pipeline avant le déploiement, puis mettre cette valeur à `false`.

Pour publier des fichiers V1, configurer la clé **publique** décrite dans
[STORE_API_V1.md](../docs/STORE_API_V1.md). La clé privée reste sur le PC développeur.
Les noms des quatre réseaux peuvent être changés via `ARZENAL_EDGE_NETWORK`,
`ARZENAL_DATABASE_NETWORK`, `ARZENAL_SCAN_NETWORK`, `ARZENAL_SCAN_UPDATES_NETWORK`, notamment pour une restauration isolée.

Les anciens dossiers `Docker/` et `Docker - 2/` sont historiques, ignorés par Git et peuvent
contenir des secrets locaux. Ils ne sont pas les sources du nouveau déploiement.

## Exploitation

La procédure de sauvegarde **MySQL + fichiers** est décrite dans [BACKUP_RESTORE.md](BACKUP_RESTORE.md).
Elle doit être exercée dans un environnement isolé avant toute mise en production.
Les volumes Docker restent sur le disque de Docker : sur le Pi, prévoir le déplacement du
répertoire Docker ou des volumes sur le disque externe, puis une copie des sauvegardes sur le PC.
La configuration réelle du proxy, du disque et des alertes doit être vérifiée sur le Pi avant lancement.
