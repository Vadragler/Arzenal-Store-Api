# Sauvegarde et restauration MySQL et fichiers

Cette procédure utilise la composition `deploy/compose.yaml`. Les fichiers de sauvegarde contiennent les données applicatives : ils doivent être chiffrés au repos et stockés hors de l'hôte de production. Ne jamais les ajouter à Git.

Une sauvegarde SQL seule ne permet pas de restaurer le Store : les MSIX/APK et les icônes
sont dans le volume `app_artifacts`. Une sauvegarde complète contient le SQL, ce volume,
les versions/digests des images et la configuration de déploiement conservée de façon privée.
Le choix provisoire est une copie sur le PC développeur ; la copie doit être automatisée
et surveillée avant la production. Le PC et le Pi dans la même pièce ne couvrent pas un sinistre du logement.

## Sauvegarde complète sur le Pi

Ces commandes sont à exécuter dans un shell Linux sur le Pi, depuis la racine du dépôt.
`/mnt/backup/arzenal-YYYYMMDD-HHMMSS` est un exemple : choisir un nouveau dossier réel sur
le disque externe. Prévoir l'espace disque et une courte interruption de l'API.

```bash
mkdir -p /mnt/backup/arzenal-YYYYMMDD-HHMMSS
docker compose --env-file deploy/.env -f deploy/compose.yaml stop api
docker compose --env-file deploy/.env -f deploy/compose.yaml exec -T mysql sh -c 'exec mysqldump --single-transaction --routines --events -u root -p"$MYSQL_ROOT_PASSWORD" --databases arzenal_store_db arzenal_auth_db' > /mnt/backup/arzenal-YYYYMMDD-HHMMSS/database.sql
docker compose --env-file deploy/.env -f deploy/compose.yaml run --rm --no-deps -T --entrypoint tar api -czf - -C /var/lib/arzenal/apps . > /mnt/backup/arzenal-YYYYMMDD-HHMMSS/artifacts.tar.gz
docker compose --env-file deploy/.env -f deploy/compose.yaml start api
sha256sum /mnt/backup/arzenal-YYYYMMDD-HHMMSS/database.sql /mnt/backup/arzenal-YYYYMMDD-HHMMSS/artifacts.tar.gz
```

Vérifier les codes de retour, la taille et le contenu de chaque fichier avant de le considérer
valide. Si une commande échoue, redémarrer l'API et traiter la sauvegarde comme incomplète.
Conserver le résultat SHA-256 avec la sauvegarde. Ne jamais supprimer la dernière sauvegarde
valide avant d'avoir testé la suivante. Transférer ensuite le dossier vers le PC par SSH/SFTP
avec un compte dédié à droits limités ; ne pas exposer SMB, MySQL ou SSH directement sur Internet.

Lors du test de restauration, lancer la composition avec `--project-name arzenal-restore`
et un fichier `.env.restore` distinct donnant quatre noms de réseaux différents de la production :

```text
ARZENAL_EDGE_NETWORK=arzenal_restore_edge
ARZENAL_DATABASE_NETWORK=arzenal_restore_database
ARZENAL_SCAN_NETWORK=arzenal_restore_scan
ARZENAL_SCAN_UPDATES_NETWORK=arzenal_restore_scan_updates
```

Ce projet crée ses propres volumes. Ne pas lui attacher les volumes de production.
Importer le SQL avec `exec -T mysql` et restaurer les fichiers avant de démarrer l'API :

```bash
docker compose --project-name arzenal-restore --env-file deploy/.env.restore -f deploy/compose.yaml up -d mysql
docker compose --project-name arzenal-restore --env-file deploy/.env.restore -f deploy/compose.yaml exec -T mysql sh -c 'exec mysql -u root -p"$MYSQL_ROOT_PASSWORD"' < /mnt/backup/arzenal-YYYYMMDD-HHMMSS/database.sql
docker compose --project-name arzenal-restore --env-file deploy/.env.restore -f deploy/compose.yaml run --rm --no-deps -T --entrypoint tar api --no-same-owner -xzf - -C /var/lib/arzenal/apps < /mnt/backup/arzenal-YYYYMMDD-HHMMSS/artifacts.tar.gz
```

Attendre que MySQL soit healthy avant l'import SQL. Après restauration, vérifier les sondes,
le login Manager, le catalogue et un téléchargement dont le SHA-256 correspond. Aucun de ces
commandes n'est lancé automatiquement par Codex sur une base existante.

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
