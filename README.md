# Arzenal-Store-Api

**Arzenal-Store-Api** est une API RESTful développée en C# avec ASP.NET Core 8.0.  
Elle permet la gestion des applications, ainsi que l’inscription, la connexion et l’authentification des utilisateurs à l’aide de JWT.

---

## ✨ Fonctionnalités principales

- 🔐 Authentification via JSON Web Token (JWT)
  - Inscription
  - Connexion
- 📦 Gestion des applications (Apps)
  - Ajout, lecture, modification, suppression
- 🗃️ Support des entités associées
  - Catégories
  - Langues
  - Systèmes d’exploitation
  - Tags

---

## ⚙️ Technologies utilisées

- ASP.NET Core 8.0
- Entity Framework Core
- MySQL
- JWT (sans refresh token pour l’instant)
- Docker (Alpine + conteneur MySQL)
- xUnit pour les tests unitaires

---

## 🧪 Environnement de développement

1. **Exécution locale** via Visual Studio :
   - Configure une base MySQL (port 3306) dans un conteneur Docker.
   - Adapter la `connection string` dans `appsettings.Development.json`.

2. **Conteneur de production** :
   - L'API tourne dans un conteneur Alpine.
   - Connexion à une base de données MySQL dans un réseau Docker privé.
   - Aucun port exposé vers l’extérieur sauf ceux requis par défaut.

---

