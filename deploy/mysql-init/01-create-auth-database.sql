CREATE DATABASE IF NOT EXISTS arzenal_auth_db;
GRANT ALL PRIVILEGES ON arzenal_auth_db.* TO 'arzenal_api'@'%';
FLUSH PRIVILEGES;
