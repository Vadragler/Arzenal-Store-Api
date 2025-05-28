# Arzenal-Store-Api

![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet?logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-blue?logo=dotnet)
![MySQL](https://img.shields.io/badge/Database-MySQL-blue?logo=mysql&logoColor=white)
![Docker](https://img.shields.io/badge/Container-Docker-blue?logo=docker)
![License](https://img.shields.io/badge/Licence-Utilisation%20interdite-red)
![Platform](https://img.shields.io/badge/Platform-Windows%20%7C%20Linux-lightgrey)

**Arzenal-Store-Api** est une API RESTful développée en C# avec ASP.NET Core 8.0.  
Elle permet la gestion des applications ainsi que l’inscription, la connexion et l’authentification des utilisateurs à l’aide de JWT.

---

## ✨ Fonctionnalités principales

- 🔐 Authentification via JSON Web Token (JWT) : inscription et connexion  
- 📦 Gestion des applications (Apps) : ajout, consultation, modification, suppression  
- 🗃️ Support des entités associées : **catégories**, **langues**, **systèmes d’exploitation**, **tags**

---

## 🛠️ Technologies utilisées

- ASP.NET Core 8.0  
- Entity Framework Core  
- MySQL  
- JWT (sans refresh token pour l’instant)  
- Docker (Alpine + conteneur MySQL)  
- xUnit pour les tests unitaires  

---

## 📦 Dépendances principales

- FluentValidation  

---

## 📜 Scripts SQL

Les scripts de création des schémas (tables, relations, contraintes) sont disponibles dans le dossier **`/database`** du dépôt :

- `schema_arzenal_auth_db.sql` — structure de la base d’authentification  
- `schema_arzenal_store_db.sql` — structure de la base de gestion des applications  

---

## ⚙️ Lancement du projet

1. Cloner ce dépôt  
2. Ouvrir la solution avec Visual Studio 2022 ou plus récent  
3. Adapter la `connection string` dans `appsettings.json`  
4. Exécuter le projet  

ℹ️ L'API nécessite que les bases de données soient disponibles, soit en local, soit dans des conteneurs Docker.

---

## 🧪 Environnement de développement

### 1. Exécution locale

- Une base MySQL (port 3306) est nécessaire — vous pouvez l'exécuter dans un conteneur Docker.  
- Adapter la chaîne de connexion dans `appsettings.json`.

### 2. Conteneur de production

- L’API tourne dans un conteneur Alpine.  
- Connexion à une base MySQL dans un réseau Docker privé.  
- Aucun port exposé vers l’extérieur, sauf ceux requis.  

---

## 🔧 À venir

- 🔁 Gestion automatique du rafraîchissement du token JWT  
