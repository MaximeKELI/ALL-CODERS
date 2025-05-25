# Système de Formulaire de Contact

Ce projet implémente un système de formulaire de contact moderne avec sauvegarde en base de données et notification par email.

## Table des matières
- [Prérequis](#prérequis)
- [Installation](#installation)
- [Configuration](#configuration)
  - [Base de données](#base-de-données)
  - [Configuration Email](#configuration-email)
- [Structure du Projet](#structure-du-projet)
- [Fonctionnalités](#fonctionnalités)
- [Guide de Dépannage](#guide-de-dépannage)
- [Sécurité](#sécurité)

## Prérequis

- .NET 7.0 ou supérieur
- Un compte Gmail (pour l'envoi d'emails)
- SQLite (inclus avec .NET)

## Installation

1. Clonez le projet :
```bash
git clone https://github.com/MaximeKELI/ALL-CODERS.git
cd backend
```

2. Installez les dépendances :
```bash
dotnet restore
```

3. Installez le package MailKit (si pas déjà fait) :
```bash
dotnet add package MailKit
```

## Configuration

### Base de données

La base de données SQLite est automatiquement créée au premier lancement. Le fichier de base de données sera créé sous le nom `app.db` dans le dossier du projet.

La configuration de la base de données se trouve dans `appsettings.json` :
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  }
}
```

### Configuration Email

1. Configuration dans `appsettings.json` :
```json
{
  "EmailSettings": {
    "Password": "votre_mot_de_passe"
  }
}
```

2. Configuration Gmail recommandée (plus sécurisée) :
   - Allez sur https://myaccount.google.com/security
   - Activez "la validation en 2 étapes"
   - Allez dans "Mots de passe des applications"
   - Créez un nouveau mot de passe d'application
   - Copiez ce mot de passe dans `appsettings.json`

## Structure du Projet

```
backend/
├── Program.cs              # Point d'entrée de l'application
├── appsettings.json       # Configuration
├── app.db                 # Base de données SQLite
└── wwwroot/              # Fichiers statiques
    ├── index.html        # Page principale
    ├── css/
    │   └── contact.css   # Styles du formulaire
    └── js/
        └── contact.js    # JavaScript du formulaire
```

## Fonctionnalités

### 1. Formulaire de Contact
- Validation en temps réel des champs
- Animations fluides
- Messages d'erreur clairs
- Protection contre les soumissions multiples

### 2. Backend
- Validation côté serveur
- Sauvegarde en base de données
- Envoi d'email
- Gestion des erreurs
- Journalisation

### 3. Email
- Format HTML responsive
- Informations complètes :
  - Nom de l'expéditeur
  - Email de l'expéditeur
  - Message
  - Date et heure
- Notification instantanée

## Guide de Dépannage

### Problèmes courants

1. **L'email n'est pas envoyé**
   - Vérifiez que le mot de passe dans `appsettings.json` est correct
   - Assurez-vous que l'authentification à 2 facteurs est activée
   - Vérifiez les logs pour plus de détails

2. **Erreurs de base de données**
   - Vérifiez les permissions du dossier pour `app.db`
   - Supprimez `app.db` et redémarrez l'application pour la recréer

3. **Le formulaire ne s'envoie pas**
   - Vérifiez la console du navigateur pour les erreurs
   - Assurez-vous que tous les champs sont remplis correctement
   - Vérifiez que le message fait au moins 10 caractères

### Logs
Les logs sont disponibles dans la console de l'application et incluent :
- Les messages reçus
- Les erreurs d'envoi d'email
- Les problèmes de base de données

## Sécurité

### Mesures de sécurité implémentées

1. **Validation des données**
   - Validation côté client
   - Validation côté serveur
   - Protection contre les injections SQL (via Entity Framework)

2. **Protection des emails**
   - Utilisation de TLS pour SMTP
   - Support des mots de passe d'application
   - Validation du format d'email

3. **Base de données**
   - Utilisation de paramètres préparés
   - Validation des entrées
   - Gestion sécurisée des connexions

### Bonnes pratiques

1. Ne jamais commiter `appsettings.json` avec des mots de passe
2. Utiliser des mots de passe d'application plutôt que le mot de passe principal
3. Mettre à jour régulièrement les packages NuGet
4. Surveiller les logs pour détecter les activités suspectes

## Démarrage

1. Configurez votre email dans `appsettings.json`
2. Lancez l'application :
```bash
dotnet run
```
3. Accédez à `http://localhost:8000` dans votre navigateur
4. Testez le formulaire de contact

## Support

Pour toute question ou problème :
1. Consultez d'abord ce README
2. Vérifiez les logs de l'application
3. Contactez le support technique si nécessaire

---

**Note importante sur la sécurité** : Ne partagez jamais vos mots de passe ou informations sensibles. Utilisez toujours des mots de passe d'application pour l'authentification Gmail. 