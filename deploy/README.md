# Déploiement Docker cloisonné

Cette composition remplace le principe d'un réseau Docker unique :

- `arzenal_edge` : le reverse proxy et l'API uniquement ;
- `arzenal_database` : l'API et MySQL uniquement ; ce réseau est `internal`, sans port publié ;
- `arzenal_malware_scan` : l'API et ClamAV uniquement ; ce réseau est aussi `internal` ;
- MySQL et ClamAV n'ont ni port publié vers l'hôte, ni réseau Docker commun : ils ne peuvent donc pas communiquer entre eux.

Le reverse proxy existant doit quitter le réseau partagé historique puis rejoindre **uniquement** `arzenal_edge`. Configure sa cible vers `http://arzenal-api:8080`. Son interface d'administration ne doit pas être publiée sur Internet ; limite-la à l'hôte, à un VPN ou à un réseau d'administration distinct.

Les outils de sécurité (ZAP, OpenVAS) ne font pas partie du déploiement applicatif permanent : lance-les dans une composition et un réseau séparés, ponctuellement. Ils ne doivent jamais partager `arzenal_database` ni `arzenal_malware_scan`.

## Mise en place

1. Construire et publier l'image API sous un tag immuable, par exemple `arzenal-store-api:1.0.0`.
2. Copier `.env.example` vers `.env` et remplacer toutes les valeurs.
3. Créer les réseaux et valider la syntaxe :

```powershell
docker compose --env-file deploy/.env -f deploy/compose.yaml config
docker compose --env-file deploy/.env -f deploy/compose.yaml up -d
```

L'API applique les migrations au démarrage uniquement parce que `Database__ApplyMigrationsOnStartup=true` dans cette composition et qu'elle ne contient qu'une instance API. Pour plusieurs réplicas, lancer les migrations une seule fois dans la pipeline avant le déploiement, puis mettre cette valeur à `false`.

## Exploitation

La procédure de sauvegarde et de restauration MySQL est décrite dans [BACKUP_RESTORE.md](BACKUP_RESTORE.md). Elle doit être exercée dans un environnement isolé avant toute mise en production.
