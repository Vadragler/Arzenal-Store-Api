# Arzenal Store API

API ASP.NET Core 10 pour le catalogue Arzenal Store, son authentification et la distribution des applications.

## Fonctionnalités

- API publique `/api/store/v1` : recherche, catégories, fiches, dernière version compatible Windows/Android.
- Releases brouillon/publiées/retirées, fichiers immuables, SHA-256 et signature de provenance de l'éditeur.
- Téléchargements publics reprenables (`Range`, `ETag`) et journal de publication côté serveur.
- Authentification JWT par cookies `HttpOnly` et refresh-tokens à usage unique.
- Mots de passe hachés avec Argon2id ; refresh-tokens hachés avant stockage.
- Gestion des applications, catégories, langues, systèmes et tags.
- Artefacts applicatifs versionnés et indexés en base : aucun téléchargement ne choisit un fichier « le plus récent » sur disque.
- Catalogue paginé via `GET /api/apps/page?page=1&pageSize=20` (maximum 100 éléments).
- Upload borné, streamé et analysé par ClamAV en production.
- Limitation de débit sur les flux sensibles.
- Documentation Swagger disponible uniquement en développement.
- Sondes `GET /health/live` et `GET /health/ready`.

## Prérequis

- SDK .NET 10.
- MySQL 8.4 LTS pour l'exécution locale et le nouveau déploiement.
- Docker Desktop avec son moteur Linux démarré pour les tests d'intégration : les bases de test sont créées automatiquement avec Testcontainers.
- ClamAV pour un environnement de production.

## Configuration locale

Le contrat V1, les commandes de publication et les limites de vérification native sont décrits
dans [docs/STORE_API_V1.md](docs/STORE_API_V1.md). Commencer par ce guide pour brancher le futur magasin.

Le modèle sans secret est dans [appsettings.example.json](ArzenalStoreApi/appsettings.example.json). Ne copiez jamais de mots de passe, clés JWT ou clé de hachage dans Git. `appsettings.Development.json` est ignoré par Git ; les secrets peuvent aussi être stockés avec User Secrets.

Les clés `Jwt:SecretKey` et `Jwt-App-Login:SecretKey` doivent contenir au moins 32 caractères. `RefreshToken:HashKey` doit être une clé aléatoire de 32 octets encodée en Base64. Par exemple, sous PowerShell :

```powershell
$refreshHashKey = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
dotnet user-secrets set --project ArzenalStoreApi "Jwt:SecretKey" "une-cle-longue-et-aleatoire-de-plus-de-32-caracteres"
dotnet user-secrets set --project ArzenalStoreApi "Jwt-App-Login:SecretKey" "une-autre-cle-longue-et-aleatoire-de-plus-de-32-caracteres"
dotnet user-secrets set --project ArzenalStoreApi "RefreshToken:HashKey" $refreshHashKey
dotnet user-secrets set --project ArzenalStoreApi "ConnectionStrings:StoreConnection" "Server=localhost;Port=3306;Database=arzenal_store_db;User Id=...;Password=...;"
dotnet user-secrets set --project ArzenalStoreApi "ConnectionStrings:AuthConnection" "Server=localhost;Port=3306;Database=arzenal_auth_db;User Id=...;Password=...;"
```

Configurez aussi `AllowedOrigins`, `CookieDomain`, `Frontend:PublicBaseUrl`, `Storage:AppFilesPath`, `AppLogin:AllowedAppIds` et, derrière un proxy, `ReverseProxy:KnownProxies`. En production, `Storage:RequireMalwareScan` et `MalwareScan:Enabled` doivent tous deux être à `true`.

## Base de données et démarrage

Appliquez les migrations pour chacun des deux contextes :

```powershell
dotnet ef database update --project ArzenalStoreInfrastructure --startup-project ArzenalStoreApi --context ApplicationDbContext
dotnet ef database update --project ArzenalStoreInfrastructure --startup-project ArzenalStoreApi --context AuthDbContext
dotnet run --project ArzenalStoreApi
```

Avec le profil HTTP par défaut, Swagger est accessible sur `http://localhost:5181/swagger`. Le port reste configurable par les profils de lancement, `ASPNETCORE_URLS` ou les variables de ports ASP.NET ; il n'est pas codé dans l'application.

Le téléchargement public V1 cible un fichier publié :

```text
GET /api/store/v1/artifacts/{artifactId}/download
```

Les anciennes routes `api/appfiles` restent réservées au Manager. Après les migrations,
configurer le slug et réimporter les anciens fichiers via V1 avant publication : aucun ancien
binaire ne devient public automatiquement.

## Tests

```powershell
dotnet test ArzenalStoreApi.TestUnitaire/Arzenal.Store.Api.TestUnitaire.csproj
dotnet test Arzenal-Store-TestApi/Arzenal.Store.Api.TestIntegration.csproj
```

Les tests d'intégration ne dépendent pas d'une base ou d'un port local : Testcontainers lance **un seul serveur MySQL 8.4 LTS par exécution**. Chaque test API dispose de ses propres bases Store/Auth, de son hôte HTTP et de son dossier de fichiers ; les migrations et les données de départ y sont appliquées avant le test. Ces ressources sont supprimées après chaque test, puis le serveur est supprimé en fin de suite, y compris depuis Visual Studio. Docker doit simplement être opérationnel.

Le démarrage du serveur est attendu de façon asynchrone avant les scénarios. La disponibilité est vérifiée par une connexion authentifiée et un `SELECT 1` depuis l'hôte sur le port Docker publié, plutôt que par le socket interne du conteneur. Cette préparation dispose de 90 secondes au maximum, avec des tentatives courtes ; sa durée réelle apparaît dans la sortie des tests (`Préparation de MySQL` puis `MySQL joignable depuis l'hôte`). Les scénarios et leurs assertions ne sont pas relancés automatiquement.

Les classes s'exécutent en parallèle, avec deux tests simultanés au maximum (`Arzenal-Store-TestApi/xunit.runner.json`). Les tests de migration utilisent aussi une base dédiée sur ce serveur. Les pools de connexions de chaque base sont fermés avant sa suppression. Le stockage MySQL en mémoire (`tmpfs`) et les réglages qui réduisent les écritures de journal concernent uniquement ces bases jetables ; la configuration de production reste distincte.

Dernière mesure locale du 9 novembre 2026 : **187 tests réussis en environ 1 min 35 s, initialisation comprise et hors compilation** (`--no-build`), dont 8,5 secondes pour préparer MySQL. L'ancienne configuration avec les conteneurs répétés prenait environ 5 min 26 s pour 182 tests. Les cinq nouveaux cas vérifient le partage du serveur, l'isolation des données et le nettoyage des bases/sessions. Le temps dépend de la machine et du téléchargement initial de l'image MySQL.

## Déploiement

- Injectez tous les secrets par le mécanisme de votre plateforme (variables d'environnement, coffre de secrets, etc.).
- Gardez MySQL, ClamAV et les interfaces d'administration sur un réseau privé.
- Exposez uniquement le reverse proxy public ; renseignez précisément ses IP dans `ReverseProxy:KnownProxies`.
- La composition Docker cloisonnée est disponible dans [deploy/README.md](deploy/README.md). Elle sépare le réseau proxy/API du réseau interne MySQL/ClamAV et ne contient aucun secret.
- Utilisez `/health/live` pour savoir si le processus est vivant et `/health/ready` pour savoir si les deux bases sont joignables.
- Consultez la CI GitHub Actions avant toute mise en production : build, tests unitaires, Testcontainers et audit NuGet y sont exécutés.
- La CI lit le dépôt privé `Arzenal-Store-Shared-Dtos`. Créer dans ce dépôt API le secret GitHub Actions `SHARED_DTOS_READ_TOKEN` : un fine-grained personal access token limité au dépôt DTO, avec la permission **Contents: Read-only** et une durée de vie courte. Ne jamais placer ce token dans un fichier versionné.
