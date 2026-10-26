# Sauvegarde et restauration MySQL

Cette procédure utilise la composition `deploy/compose.yaml`. Les fichiers de sauvegarde contiennent les données applicatives : ils doivent être chiffrés au repos et stockés hors de l'hôte de production. Ne jamais les ajouter à Git.

## Sauvegarde

Exécuter cette commande depuis la racine du dépôt, avec `deploy/.env` présent :

```powershell
New-Item -ItemType Directory -Force backups | Out-Null
docker compose --env-file deploy/.env -f deploy/compose.yaml exec -T mysql sh -c 'exec mysqldump --single-transaction --routines --events -u root -p"$MYSQL_ROOT_PASSWORD" --databases arzenal_store_db arzenal_auth_db' > ("backups/arzenal-" + (Get-Date -Format yyyyMMdd-HHmmss) + ".sql")
```

Conserver aussi la version de l'image MySQL et vérifier que le fichier créé n'est pas vide. Tester cette sauvegarde régulièrement sur une machine ou un environnement isolé.

## Restauration vérifiée dans un environnement isolé

1. Arrêter l'API de l'environnement de restauration, sans toucher à la production.
2. Créer des volumes et un fichier `.env` propres à cet environnement.
3. Démarrer uniquement MySQL, attendre son état `healthy`, puis importer le dump :

```powershell
Get-Content backups/arzenal-YYYYMMDD-HHMMSS.sql -Raw | docker compose --env-file deploy/.env -f deploy/compose.yaml exec -T mysql sh -c 'exec mysql -u root -p"$MYSQL_ROOT_PASSWORD"'
```

4. Démarrer l'API, vérifier `/health/ready`, se connecter avec un compte de test et vérifier la présence des applications et artefacts attendus.

## Restauration de production

Une restauration de production remplace des données : elle doit être décidée explicitement, avec une sauvegarde fraîche de l'état courant avant toute importation. Désactiver l'API ou placer le site en maintenance pour empêcher des écritures pendant l'opération. Après importation, redémarrer l'API et vérifier `/health/ready`.
