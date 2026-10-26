# Audit technique — Arzenal-Store-Api

Date de l'audit : 2 septembre 2026 (mis à jour le 26 octobre 2026)
Périmètre : état courant du dépôt, y compris les modifications locales non validées, et le projet DTO partagé référencé par la solution.

## Mise à jour de sécurisation — 26 octobre 2026

Les éléments **SEC-01 à SEC-08 sont implémentés** dans l'arbre de travail : identité extraite du JWT, app-id autorisé, refresh-tokens HMAC en base avec familles, expiration absolue et détection de réutilisation, upload confiné et contrôlé, erreurs publiques génériques, rate limiting, index d'unicité et contraintes de base. Les tests unitaires passent à **274/274**. La migration `SecureRefreshTokensAndUserIndexes` invalide volontairement les refresh-tokens existants : tous les utilisateurs devront se reconnecter après son application.

La note de sécurité passe de **5/20 à 15/20**. La note générale passe à **14/20** : les principaux risques applicatifs ont été retirés, mais DEP-01, DEP-02, la CI, les health checks réels et l'industrialisation restent à traiter avant une exposition publique.

## Mise à jour de fiabilité et industrialisation — 26 octobre 2026

Les packages système redondants et les dépendances ASP.NET 2.3 ont été retirés ; EF Core est aligné en 9.0.14 pour rester compatible avec Pomelo 9.0.0, et SSH.NET est explicitement contraint en 2026.0.0 afin de corriger la dépendance vulnérable de Testcontainers. Swagger est réactivé en environnement Development avec Microsoft.OpenApi 2.7.5, version compatible résolue par Swashbuckle 10.3.1 ; les endpoints `/health/live` et `/health/ready` sont en place, le port Kestrel n'est plus codé en dur et `IHttpContextAccessor` est explicitement enregistré.

Une CI GitHub Actions exécute désormais restore, build, tests unitaires, tests d'intégration MySQL/Testcontainers et l'audit NuGet High/Critical. La validation locale a confirmé 283/283 tests unitaires et 127/127 tests d'intégration, sans vulnérabilité NuGet signalée pour l'API ni le projet d'intégration. Le README décrit la configuration sans secret, les migrations, les tests et le déploiement. Les contrats et mappers ont également été nettoyés afin que les compilations applicatives soient sans avertissement.

## Verdict général

**Note actuelle : 17/20.** Le projet est désormais adapté à une petite production privée : authentification et refresh-tokens durcis, upload borné, artefacts versionnés, test d'intégration MySQL isolé, CI, documentation et endpoints d'exploitation minimaux sont présents.

Il reste toutefois des chantiers avant une exposition publique large : la sélection des artefacts téléchargés reste pilotée par le disque, les listes ne sont pas paginées, l'architecture est encore directement couplée à EF Core et le déploiement Docker local doit être normalisé sans secrets en clair. La note reflète l'état du code actuel et non une garantie de sécurité absolue.

| Axe | Note | Synthèse |
| --- | ---: | --- |
| Architecture et lisibilité | 13/20 | Découpage utile, mais services couplés à EF Core et domaine couplé aux DTO. |
| Sécurité | 15/20 | Les risques majeurs initiaux sont corrigés ; le déploiement doit encore préserver les secrets. |
| Qualité et tests | 17/20 | Tests unitaires et intégration isolée, smoke tests et CI présents. |
| Exploitabilité / déploiement | 16/20 | Health checks, configuration de port et composition Docker cloisonnée sont présents ; le basculement réel reste à effectuer. |
| Documentation et maintenance | 16/20 | README et dépendances sont alignés, CI ajoutée. |

## Méthode et limites

L'audit a été réalisé par lecture statique du code, de la solution, des configurations disponibles, des migrations, des fichiers Docker et du graphe NuGet. Les tests unitaires ont été exécutés avec succès (283/283) et les tests d'intégration avec succès (127/127). Les tests d'intégration utilisent désormais Testcontainers : aucune base, aucun port ni credential local ne sont requis ; le moteur Docker est la seule dépendance externe.

Le dépôt contenait déjà **52 entrées non propres** dans `git status` avant la création de ce document (modifications de code/tests et nouveaux dossiers). Elles ne sont pas attribuées à cet audit. Seul ce fichier `PLAN.md` est ajouté.

## Points positifs à conserver

- La politique d'autorisation de secours impose une authentification par défaut ; les endpoints publics sont explicitement annotés.
- Les cookies d'authentification sont `HttpOnly`, `Secure` et `SameSite=Strict`.
- Les mots de passe sont hachés avec Argon2id, et les tokens de refresh sont générés par une source cryptographique aléatoire.
- Les opérations EF Core utilisent généralement l'asynchronisme et les lectures utilisent parfois `AsNoTracking`.
- Deux contextes distincts séparent les données métier des données d'authentification.
- Une réelle intention de couvrir les contrôleurs, services, validateurs et l'authentification est visible dans les projets de test.

## Constats prioritaires

### P0 — à corriger avant toute exposition

| ID | Constat et impact | Éléments observés | Correctif attendu |
| --- | --- | --- | --- |
| SEC-01 — résolu | L'identité du refresh-token est désormais extraite uniquement du claim `NameIdentifier` du JWT `AppLogin`; le DTO public ne porte plus `UserId`, IP ou user-agent. `X-App-Id` doit appartenir à la liste `AppLogin:AllowedAppIds`. | Contrôleur Auth, `AuthService`, `JwtService` et tests de régression. | Maintenir la liste d'applications autorisées dans la configuration de déploiement. |
| SEC-02 — résolu | **HMAC personnalisé retiré.** Middleware, attribut, helper, interface/service, en-têtes de signature et tests dédiés ont été supprimés. Les clients WPF et Angular ne contenaient aucune référence restante. | Retrait effectué le 2 septembre 2026. | Si la clé a déjà été déployée ou partagée, la considérer compromise et renouveler les secrets/sessions concernés. Conserver JWT, les policies et HTTPS comme mécanismes d'autorisation. |

### P1 — à corriger dans la première itération de sécurisation

| ID | Constat et impact | Éléments observés | Correctif attendu |
| --- | --- | --- | --- |
| SEC-03 — résolu | Les refresh-tokens sont HMAC-SHA-256, jamais stockés en clair. La rotation impose une correspondance exacte, conserve une famille, détecte une réutilisation, révoque la famille et s'appuie sur un jeton de concurrence/index unique. | `RefreshTokenService`, `HmacRefreshTokenHasher`, modèle et migration Auth. | Préserver `RefreshToken:HashKey` dans un gestionnaire de secrets : sa perte invalide les sessions. |
| SEC-04 — résolu | Les chemins sont bornés sous le répertoire de stockage; les segments, plateforme, type et extension sont validés et les noms physiques sont générés côté serveur. | `AppFilesService`. | Ajouter à terme une table d'artefacts pour ne plus sélectionner le plus récent sur disque. |
| SEC-05 — résolu | Upload streamé, plafonné à 256 Mo, limité en débit, signatures binaires contrôlées et écriture temporaire atomique. ClamAV est obligatoire en production. | `AppFilesController`, `AppFilesService`, `ClamAvFileScanner`. | Déployer un service ClamAV accessible sur l'hôte configuré avant le passage en production. |
| SEC-06 — résolu | Un seul middleware `ProblemDetails` retourne des messages sûrs et un `traceId`; les détails restent exclusivement dans les logs. | `ExceptionHandlingMiddleware`. | Centraliser ensuite les logs dans l'outil d'observabilité choisi. |
| SEC-07 — résolu | Des politiques de rate limit protègent login, refresh, invitations, app-login et upload. Les refus sont journalisés; les headers transférés ne sont pris en compte que pour les proxies connus. | `Program.cs` et contrôleurs. | Déclarer les adresses réelles du reverse proxy dans `ReverseProxy:KnownProxies`. |
| SEC-08 — résolu | Unicité base de données sur email, username et hash de refresh-token, index de recherche et contrôle de doublon lors du patch. | `AuthDbContext`, migration Auth et `UserService`. | Les éventuels doublons historiques doivent être résolus avant l'application de la migration. |
| DEP-01 — correction prête au basculement | Une composition versionnée dans `deploy/` applique le moindre privilège réseau : `arzenal_edge` (proxy/API), `arzenal_database` (API/MySQL) et `arzenal_malware_scan` (API/ClamAV), tous les réseaux sensibles étant internes. MySQL et ClamAV ne partagent donc aucun réseau. Aucun port MySQL/ClamAV n'est publié, aucune image n'utilise `latest` et les valeurs viennent d'un `.env` ignoré. La CI valide la syntaxe. | `deploy/compose.yaml`, `.env.example`, `mysql-init/` et `deploy/README.md`. Les anciens dossiers Docker restent volontairement intacts : ils contiennent des données et secrets locaux. | Basculer le reverse proxy existant vers `arzenal_edge`, recréer les secrets, retirer les ports d'administration publics et archiver les anciennes compositions après vérification du démarrage réel. |
| DEP-02 — résolu | Microsoft.OpenApi 2.7.5 est résolu transitivement par Swashbuckle 10.3.1 (une référence directe 3.11 était incompatible) et SSH.NET est explicitement fixé en 2026.0.0 pour Testcontainers. Les packages système et ASP.NET historiques inutiles ont été retirés ; EF Core 9.0.14 reste cohérent avec Pomelo 9.0.0. | Projets API, Service, DTO, Unit et Integration ; `Directory.Packages.props`. | La CI bloque désormais les vulnérabilités NuGet High/Critical. Refaire l'audit à chaque évolution majeure de dépendance. |

### P2 — fiabilité, performance et maintenabilité

| ID | Constat | Correctif attendu |
| --- | --- | --- |
| REL-01 — résolu | Chaque upload crée ou met à jour un `AppArtifact` avec version, plateforme, chemin relatif contrôlé, type MIME et taille. Le téléchargement exige maintenant l'application, la version et la plateforme. | Migration `AddAppArtifactMetadata`, `AppFilesService`, route `/api/appfiles/download/{appId}/versions/{version}/platforms/{platform}` et tests unitaires/intégration. |
| REL-02 — résolu | La protection HMAC était appliquée de façon incohérente aux opérations mutantes. Elle a été retirée uniformément : ces opérations reposent désormais sur les policies JWT déjà en place. | Ajouter des tests de matrice lecture/création/modification/suppression afin de vérifier que chaque endpoint exige le rôle approprié. |
| REL-03 — résolu | Swagger est enregistré et servi en Development/Testing seulement. Un test d'intégration vérifie la disponibilité de `/swagger/v1/swagger.json`. | Conserver l'UI limitée aux environnements non publics ; documenter les mécanismes d'authentification si une UI publique est un jour voulue. |
| REL-04 — résolu | `AddHttpContextAccessor()` est explicitement enregistré. La dépendance HMAC supprimée ne subsiste plus. | Maintenir un test de démarrage/smoke à chaque évolution du conteneur DI. |
| PERF-01 — partiellement résolu | Le catalogue possède `GET /api/apps/page` avec tri stable, plafond de 100, `AsNoTracking`, requêtes EF séparées et `CancellationToken`. Les uploads sont déjà streamés. | Migrer les listes de référentiels vers le même contrat lorsque leur volume le justifiera, puis retirer l'ancien endpoint non paginé après mise à jour des clients. |
| DATA-01 — résolu | `ApplicationDbContext` ne force plus la journalisation SQL ni les données sensibles lorsqu'il est construit sans options. | N'activer des logs SQL détaillés qu'à la demande, localement, et sans données personnelles. |
| ARCH-01 — partiellement résolu | Le projet Domain ne référence plus les DTO partagés : il ne dépend que de ses modèles propres. | La couche Service dépend encore directement des `DbContext`. Introduire des ports de persistance progressivement lors de futures évolutions métier plutôt qu'une refonte risquée sans besoin fonctionnel. |
| MAINT-01 — en grande partie résolu | Les packages historiques et inutiles ont été retirés ; les versions EF sont cohérentes avec Pomelo. | Central Package Management à l'échelle de tous les projets reste une amélioration de confort, pas un risque bloquant. |
| MAINT-02 — en grande partie résolu | README aligné sur .NET 10, refresh-tokens, migrations, Testcontainers et secrets. Un workflow CI bloque les avertissements, vérifie le format, compile, teste, produit une couverture Cobertura et audite NuGet. | Ajouter une règle d'analyse statique ciblée, un scan d'image et la publication/lecture du seuil de couverture au workflow. |
| OBS-01 — partiellement résolu | `/health/live` confirme la disponibilité du processus et `/health/ready` vérifie réellement les deux bases. Le port est configuré par l'hôte au lieu d'être codé en dur. | Ajouter métriques, traces structurées et alertes avant une exposition importante. |

## Plan d'exécution restant

Les lots SEC, la remise en état des dépendances, la CI, Swagger et les health checks ont été réalisés. Les priorités qui restent concernent principalement le déploiement Docker local, le stockage d'artefacts, la pagination et l'architecture.

### Lot 0 — déploiement à assainir

1. Mettre les interfaces d'administration Docker hors exposition publique et remplacer les mots de passe locaux si ces environnements ont été accessibles.
2. Créer une composition versionnée sans secrets, accompagnée d'un `.env.example` et de noms de configuration conformes à `StoreConnection`, `AuthConnection` et `Storage`.
3. Épingler les images de production par version et digest, puis vérifier le démarrage de cette composition dans la CI.

### Lot 1 — artefacts et performances

1. Créer une table `AppArtifact` indexée par application, version, plateforme et type.
2. Faire télécharger explicitement un artefact référencé en base, jamais le premier fichier du disque.
3. Ajouter une pagination bornée et des `CancellationToken` aux listes susceptibles de croître.
4. Mesurer les endpoints les plus utilisés avant d'optimiser les requêtes secondaires.

### Lot 2 — observabilité et architecture

1. Ajouter métriques, traces et alertes sur les erreurs d'authentification, les scans antivirus et la disponibilité MySQL.
2. Découpler progressivement la couche Service des `DbContext` et le domaine des DTO exposés.
3. Ajouter des tests de charge pour l'upload et exercer périodiquement la procédure de sauvegarde/restauration MySQL documentée dans `deploy/BACKUP_RESTORE.md`.

### Lot 3 — CI avancée

1. Ajouter une analyse statique ciblée, un scan d'image et définir un seuil de couverture justifié à la CI actuelle ; le format, les avertissements bloquants et un rapport de couverture sont déjà appliqués/produits.
2. Faire échouer la CI sur une régression des smoke tests de démarrage et de health checks.
3. Réviser périodiquement les dépendances, les accès administrateur et les secrets de déploiement.

## Critères de sortie avant production

- Aucun P0/P1 ouvert et les secrets ne figurent ni dans le code ni dans les fichiers versionnés.
- Une identité de session ne peut être déduite que d'un token validé, jamais d'un `UserId` reçu du client.
- Les refresh-tokens sont hachés, à usage unique, atomiques et couverts par des tests de réutilisation/concurrence.
- Un upload ne peut ni sortir du stockage, ni saturer la mémoire, ni accepter un contenu non autorisé.
- Les dépendances High sont corrigées ou ont une dérogation formelle et temporaire.
- La CI verte exécute build, tests et scans ; un test de démarrage valide la configuration de production simulée.
- Les erreurs client ne divulguent pas les détails techniques et les logs/health checks permettent le diagnostic côté exploitation.
