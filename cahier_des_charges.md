# Cahier des charges — Application web de tri automatique des emails par projet

## 1. Contexte du projet

Le projet consiste à développer une application web permettant à des utilisateurs ou organisations de connecter leurs comptes Gmail et/ou Outlook afin de trier automatiquement leurs emails entrants selon les projets détectés dans l’objet du message.

L’utilisateur configure une liste de projets et, pour chaque projet, une liste éventuelle d’alias. Lorsqu’un nouvel email arrive, l’application reçoit un événement via Gmail Pub/Sub ou Microsoft Graph Webhooks, analyse le sujet du mail, détecte le projet correspondant, puis applique l’action de classement adaptée au fournisseur mail.

Pour le MVP, le classement repose sur une recherche simple et déterministe du nom de projet ou de ses alias dans le titre de l’email après normalisation. Le système doit cependant être conçu pour permettre l’ajout ultérieur de stratégies de classification plus avancées : règles personnalisées, expressions régulières, score par mots-clés, IA, LLM local ou distant.

## 2. Objectifs du MVP

Le MVP doit permettre à un utilisateur de :

1. Créer un compte ou se connecter à l’application.
2. Créer ou rejoindre un espace d’organisation.
3. Connecter un compte Gmail.
4. Connecter un compte Outlook / Microsoft 365.
5. Déclarer des projets.
6. Ajouter des alias par projet.
7. Activer ou désactiver le tri automatique par compte mail.
8. Recevoir les événements de nouveaux emails via Pub/Sub ou Webhooks.
9. Classer automatiquement les emails entrants selon le projet détecté dans le sujet.
10. Consulter l’historique des traitements réalisés.
11. Comprendre pourquoi un mail a été classé, ignoré ou mis en erreur.
12. Déconnecter un compte mail et supprimer les autorisations associées.

Le MVP doit intégrer directement les mécanismes événementiels Gmail Pub/Sub et Microsoft Graph Webhooks. Le polling ne doit pas être utilisé comme mécanisme principal de production.

## 3. Périmètre fonctionnel

### 3.1 Gestion des utilisateurs

L’application doit permettre :

* la création de compte ;
* la connexion sécurisée ;
* la déconnexion ;
* la gestion du profil utilisateur ;
* la gestion de l’appartenance à une organisation ;
* la gestion des rôles simples.

Rôles minimaux attendus :

* propriétaire de l’organisation ;
* administrateur ;
* utilisateur standard.

Le propriétaire ou administrateur doit pouvoir gérer les projets, les comptes mails connectés et les règles de classement de l’organisation ou de son propre espace selon le modèle retenu.

### 3.2 Authentification applicative

L’authentification de l’application doit être gérée via Keycloak.

Exigences :

* support OpenID Connect ;
* gestion des sessions ;
* gestion des rôles ;
* support de l’authentification multifacteur si activée ;
* séparation claire entre authentification à l’application et autorisation d’accès aux comptes Gmail/Outlook ;
* aucune gestion artisanale des mots de passe dans l’application métier.

### 3.3 Connexion Gmail

L’application doit permettre à un utilisateur d’autoriser l’accès à son compte Gmail via OAuth.

Exigences :

* utiliser les scopes minimaux nécessaires ;
* permettre la révocation de l’accès ;
* stocker les tokens de manière chiffrée ;
* renouveler les tokens lorsque nécessaire ;
* ne pas demander l’accès au contenu complet des emails si le sujet et les métadonnées suffisent ;
* créer ou récupérer les labels nécessaires ;
* appliquer le label correspondant au projet détecté ;
* optionnellement retirer le mail de la boîte de réception si l’utilisateur active l’option d’archivage automatique.

Comportement attendu pour Gmail :

* un projet correspond à un label ;
* un mail peut recevoir un ou plusieurs labels si plusieurs projets sont détectés selon la configuration retenue ;
* le système doit éviter de recréer plusieurs fois le même label ;
* le système doit conserver une correspondance entre projet interne et label Gmail.

### 3.4 Connexion Outlook / Microsoft 365

L’application doit permettre à un utilisateur d’autoriser l’accès à sa boîte Outlook ou Microsoft 365 via OAuth Microsoft.

Exigences :

* utiliser Microsoft Graph ;
* utiliser les permissions minimales nécessaires ;
* permettre la révocation de l’accès ;
* stocker les tokens de manière chiffrée ;
* renouveler les tokens lorsque nécessaire ;
* créer ou récupérer les dossiers nécessaires ;
* déplacer l’email vers le dossier correspondant au projet détecté ;
* conserver une correspondance entre projet interne et dossier Outlook.

Comportement attendu pour Outlook :

* un projet correspond à un dossier ;
* si plusieurs projets sont détectés, le comportement doit être configurable ou déterministe ;
* le système doit éviter les déplacements multiples incohérents ;
* le système doit tracer l’action réellement exécutée.

### 3.5 Gestion des projets

L’utilisateur doit pouvoir :

* créer un projet ;
* modifier un projet ;
* désactiver un projet ;
* supprimer un projet si aucune contrainte ne l’empêche ;
* ajouter des alias ;
* modifier les alias ;
* désactiver certains alias ;
* définir le nom cible du label ou dossier ;
* tester manuellement un sujet d’email contre les règles existantes.

Un projet doit contenir au minimum :

* nom ;
* description optionnelle ;
* statut actif/inactif ;
* liste d’alias ;
* nom cible de classement ;
* date de création ;
* date de modification ;
* propriétaire ou organisation associée.

### 3.6 Matching des emails

Pour le MVP, la stratégie de matching obligatoire est :

* récupérer le sujet de l’email ;
* normaliser le sujet ;
* normaliser les noms de projets et alias ;
* vérifier si le sujet contient un nom de projet ou alias ;
* retourner le projet correspondant ;
* exécuter l’action de classement.

La normalisation doit au minimum gérer :

* casse ;
* accents ;
* espaces multiples ;
* caractères spéciaux simples ;
* préfixes de réponse ou transfert tels que RE:, FW:, FWD:, TR:.

Le moteur de matching doit être extensible. Le code ne doit pas lier directement le traitement mail à une seule méthode de comparaison. Il doit être possible d’ajouter ultérieurement d’autres stratégies sans réécrire le pipeline complet.

Stratégies futures à anticiper :

* alias avancés ;
* expressions régulières ;
* règles avec priorité ;
* règles par expéditeur ;
* règles combinées sujet + expéditeur ;
* score par mots-clés ;
* classification IA ;
* classification hybride règle + IA.

### 3.7 Gestion des conflits de matching

Le système doit gérer explicitement les cas suivants :

* aucun projet détecté ;
* un seul projet détecté ;
* plusieurs projets détectés ;
* projet désactivé ;
* alias ambigu ;
* action impossible côté fournisseur ;
* email déjà traité.

Pour le MVP, les comportements minimaux attendus sont :

* aucun projet détecté : ne pas classer, tracer l’événement ;
* un projet détecté : classer ;
* plusieurs projets détectés : appliquer une règle déterministe documentée, par exemple priorité au projet le plus spécifique ou statut “conflit” sans action ;
* email déjà traité : ne pas retraiter ;
* erreur fournisseur : historiser l’erreur et permettre une reprise.

### 3.8 Réception événementielle des emails

Le MVP doit intégrer :

* Gmail Pub/Sub pour Gmail ;
* Microsoft Graph Webhooks pour Outlook / Microsoft 365.

Le système doit :

* recevoir les notifications entrantes ;
* valider leur authenticité autant que possible ;
* identifier le compte mail concerné ;
* récupérer uniquement les informations nécessaires sur le mail ;
* créer une tâche de traitement ;
* garantir que le même email n’est pas traité plusieurs fois ;
* gérer les erreurs temporaires ;
* journaliser chaque événement utile.

Si une file d’attente est nécessaire, RabbitMQ doit être utilisé.

### 3.9 File d’attente et traitement asynchrone

Si RabbitMQ est intégré dès le MVP, il doit permettre :

* la séparation entre réception d’événement et traitement métier ;
* la reprise sur erreur ;
* la gestion des retries ;
* l’isolation des traitements longs ;
* la non-perte des événements ;
* la traçabilité du statut des jobs.

Chaque tâche de traitement doit contenir uniquement les informations nécessaires :

* identifiant interne du compte mail ;
* fournisseur ;
* identifiant externe du message ;
* type d’événement ;
* horodatage ;
* identifiant de corrélation.

Le contenu complet du mail ne doit pas être placé dans la queue.

### 3.11 Journalisation technique

Le système doit conserver des logs techniques permettant de diagnostiquer :

* réception d’événement ;
* job créé ;
* job traité ;
* projet détecté ;
* action exécutée ;
* erreur fournisseur ;
* erreur OAuth ;
* erreur de configuration ;
* tentative de retraitement.

Les logs ne doivent pas contenir le corps des emails. Les sujets d’emails doivent être considérés comme des données personnelles et traités avec prudence.

### 3.12 Paramètres utilisateur

L’utilisateur doit pouvoir configurer :

* activation/désactivation du tri automatique ;
* fournisseur actif ou non ;
* comportement si aucun projet n’est trouvé ;
* comportement si plusieurs projets sont trouvés ;

### 3.13 Déconnexion et suppression

L’utilisateur doit pouvoir :

* déconnecter Gmail ;
* déconnecter Outlook ;
* supprimer les tokens associés ;
* désactiver le traitement automatique ;
* supprimer son compte ;
* demander la suppression de ses données personnelles ;
* exporter les données de configuration le concernant.

La suppression doit être effective sur les données applicatives, sauf conservation justifiée par des obligations légales ou de sécurité clairement documentées.

## 5. Stack technique imposée

### 5.1 Frontend

* React
* TypeScript obligatoire
* Gestion propre des formulaires
* Validation côté client et côté serveur
* Appels API typés autant que possible
* Interface responsive
* Design sobre et professionnel

Librairies UI possibles selon choix équipe :

* Material UI
* Ant Design
* Shadcn/ui
* Tailwind CSS

Le choix final doit être cohérent, documenté et maintenu sur tout le projet.

### 5.2 Backend

* ASP.NET Core
* C#
* API REST
* Worker .NET pour traitement asynchrone
* Entity Framework Core ou autre ORM validé par l’équipe
* Tests automatisés obligatoires sur la logique métier sensible

### 5.3 Base de données

* PostgreSQL

La base doit stocker :

* utilisateurs applicatifs référencés ;
* organisations ;
* rôles applicatifs ;
* comptes mails connectés ;
* projets ;
* alias ;
* configuration ;
* erreurs fonctionnelles ;
* métadonnées nécessaires.

### 5.4 Authentification

* Keycloak
* OpenID Connect
* OAuth2

Keycloak sert uniquement à l’authentification et à la gestion d’identité applicative. Les autorisations Gmail et Microsoft sont des flux OAuth distincts.

### 5.5 Intégrations email

* Gmail API
* Gmail Pub/Sub
* Microsoft Graph API
* Microsoft Graph Webhooks

Le système doit être prévu pour supporter plusieurs comptes mails par utilisateur ou par organisation.

### 5.6 Queue

* RabbitMQ si file d’attente intégrée

La queue doit être utilisée pour découpler la réception d’événement et le traitement effectif.

### 5.7 Déploiement

* Docker obligatoire
* Docker Compose pour environnement de développement
* configuration par variables d’environnement
* secrets séparés des fichiers versionnés
* documentation d’installation locale
* documentation d’installation serveur

### 5.8 Observabilité

Le projet doit prévoir :

* logs structurés ;
* identifiant de corrélation par événement ;
* métriques techniques ;
* suivi des erreurs ;
* visibilité sur les jobs échoués ;
* endpoint de santé ;
* endpoint de readiness si pertinent.

## 6. Exigences de qualité du code

### 6.1 Principes généraux

Le code doit être :

* lisible ;
* testé ;
* maintenable ;
* documenté lorsque nécessaire ;
* découpé par responsabilité ;
* indépendant autant que possible des fournisseurs externes ;
* extensible sans réécriture massive.

### 6.2 Séparation des responsabilités

Le code doit clairement séparer :

* contrôleurs API ;
* services applicatifs ;
* domaine métier ;
* accès aux données ;
* intégrations externes ;
* stratégies de matching ;
* actions de classement ;
* sécurité ;
* configuration ;
* observabilité.

Aucune logique métier critique ne doit être enfouie dans les contrôleurs API.

### 6.3 Extensibilité

Le système doit permettre d’ajouter :

* un nouveau fournisseur mail ;
* une nouvelle stratégie de matching ;
* une nouvelle action de classement ;
* une nouvelle règle ;
* une nouvelle politique de conflit ;
* un futur moteur IA ;
* un futur module de workflow.

L’ajout d’une nouvelle stratégie de matching ne doit pas imposer de modifier le cœur du pipeline de traitement.

### 6.4 Patterns attendus

L’équipe doit prévoir l’utilisation de patterns adaptés, notamment :

* Strategy pour les stratégies de matching ;
* Adapter pour les fournisseurs Gmail et Outlook ;
* Factory si nécessaire pour sélectionner une stratégie ;
* Repository ou équivalent pour l’accès aux données ;
* Background Worker pour les traitements asynchrones ;
* DTO pour les échanges API ;

## 7. Interface utilisateur attendue

### 7.1 Pages minimales

Le MVP doit contenir :

* page de connexion ;
* tableau de bord ;
* page projets ;
* page création/modification projet ;
* page alias ;
* page comptes mails connectés ;
* page connexion Gmail ;
* page connexion Outlook ;
* page paramètres ;
* page confidentialité / gestion des données ;

### 7.2 Tableau de bord

Le tableau de bord doit afficher :

* nombre de comptes connectés ;
* nombre de projets actifs ;
* statut des intégrations Gmail/Outlook ;
* statut du traitement automatique.

## 8. Exigences de sécurité

### 8.1 Accès

* authentification obligatoire ;
* contrôle d’accès par rôle ;
* vérification côté backend ;
* interdiction de faire confiance uniquement au frontend ;
* cloisonnement strict entre organisations ;
* interdiction pour un utilisateur d’accéder aux données d’un autre tenant.

### 8.2 Secrets

* aucun secret dans le dépôt Git ;
* aucun token dans les logs ;
* secrets via variables d’environnement ou coffre de secrets ;
* rotation possible ;
* chiffrement des tokens OAuth ;
* accès restreint aux secrets en production.

### 8.3 Données mails

* ne pas stocker le corps des emails dans le MVP ;
* ne pas stocker les pièces jointes ;
* stocker uniquement les métadonnées nécessaires ;
* considérer le sujet comme une donnée personnelle ;
* prévoir une purge automatique.

## 9. Critères d’acceptation du MVP

Le MVP sera considéré comme acceptable si :

1. Un utilisateur peut se connecter via Keycloak.
2. Un utilisateur peut connecter Gmail via OAuth.
3. Un utilisateur peut connecter Outlook via OAuth.
4. Un utilisateur peut créer plusieurs projets.
5. Un utilisateur peut ajouter plusieurs alias par projet.
6. Gmail Pub/Sub déclenche le traitement d’un nouvel email.
7. Microsoft Graph Webhook déclenche le traitement d’un nouvel email.
8. Un email Gmail entrant est labellisé automatiquement si son sujet contient un projet ou alias.
9. Un email Outlook entrant est déplacé automatiquement si son sujet contient un projet ou alias.
10. Aucun email n’est traité deux fois.
11. Les erreurs sont visibles dans l’historique.
12. L’utilisateur peut déconnecter Gmail ou Outlook.
13. Les tokens sont supprimés ou rendus inutilisables après déconnexion.
14. Les logs ne contiennent pas de tokens ni de corps d’emails.
15. Les tests couvrent les principaux cas de matching.
16. Le projet peut être lancé localement via Docker Compose.
17. Les variables de configuration sont documentées.
18. Les traitements sont traçables.
19. Les données sont cloisonnées entre utilisateurs ou organisations.
20. La documentation développeur permet de reprendre le projet sans connaissance orale préalable.

## 11. Évolutions prévues après MVP

Évolutions possibles :

* moteur de règles avancées ;
* priorités entre règles ;
* conditions sur expéditeur ;
* conditions sur destinataire ;
* conditions sur mots-clés ;
* expressions régulières ;
* score de confiance ;
* classification IA ;
* suggestion automatique de projet ;
* apprentissage à partir des corrections utilisateur ;
* intégration n8n pour workflows spécifiques ;
* notifications Slack/Teams/Discord ;
* rapports hebdomadaires ;
* mode auto-hébergé client ;
* connecteurs supplémentaires ;
* console administrateur avancée ;
* mode multi-tenant complet pour PME.

## 12. Documentation attendue

L’équipe doit livrer :

* README de lancement local ;
* documentation des variables d’environnement ;
* documentation des flux OAuth Gmail ;
* documentation des flux OAuth Microsoft ;
* documentation Pub/Sub Gmail ;
* documentation Webhooks Microsoft ;
* documentation des modèles de données ;
* documentation des stratégies de matching ;
* documentation de sécurité ;
* documentation RGPD minimale ;
* procédure de suppression des données ;
* procédure de révocation des tokens ;
* procédure de gestion d’incident ;
* guide de déploiement.

## 13. Livrables attendus

Livrables MVP :

* frontend React ;
* backend ASP.NET Core ;
* worker de traitement ;
* intégration Keycloak ;
* intégration Gmail ;
* intégration Outlook ;
* intégration Pub/Sub Gmail ;
* intégration Microsoft Graph Webhooks ;
* base PostgreSQL ;
* RabbitMQ si retenu ;
* Docker Compose ;
* tests automatisés ;
* documentation technique ;
* documentation RGPD minimale ;
* scripts de migration ;
* exemples de configuration ;
* jeu de données de test non sensible.

## 14. Contraintes de conception

L’équipe est libre de proposer l’architecture technique détaillée, mais doit respecter les contraintes suivantes :

* React côté frontend ;
* ASP.NET Core côté backend ;
* PostgreSQL comme base principale ;
* Keycloak pour l’authentification applicative ;
* Gmail Pub/Sub et Microsoft Graph Webhooks dès le MVP ;
* RabbitMQ si file d’attente ;
* extensibilité des stratégies de matching ;
* minimisation des données personnelles ;
* non-stockage du contenu complet des emails dans le MVP ;
* chiffrement des tokens OAuth ;
* logs sans secrets ;
* documentation claire ;
* tests automatisés sur le cœur métier.

## 15. Définition de “terminé”

Une fonctionnalité est considérée comme terminée uniquement si :

* elle est développée ;
* elle est testée ;
* elle est documentée ;
* elle respecte les exigences de sécurité ;
* elle respecte les exigences RGPD applicables ;
* elle fonctionne en local via Docker ;
* elle ne casse pas les tests existants ;
* elle est relue par au moins un autre développeur ;
* elle ne stocke pas plus de données que nécessaire ;
* elle est observable en cas d’erreur.
