# MailAssistant — feuille de route du MVP

## 1. Objectif

Construire une application web multi-tenant qui connecte des comptes Gmail et
Microsoft 365, détecte un projet à partir du sujet des nouveaux messages, puis
classe les messages dans un label Gmail ou un dossier Outlook.

Cette feuille de route découpe le travail en itérations courtes et vérifiables.
L’état réellement atteint est suivi dans `PROJECT_STATE.md`.

## 2. Analyse du cahier des charges

### Périmètre fonctionnel du MVP

- Authentification applicative avec Keycloak.
- Organisations et rôles `Owner`, `Admin` et `Member`.
- Gestion des projets, alias et paramètres de classement.
- Connexion OAuth de plusieurs comptes Gmail et Microsoft 365.
- Réception événementielle via Gmail Pub/Sub et Microsoft Graph Webhooks.
- Traitement asynchrone, idempotent et relançable.
- Matching déterministe et extensible sur le sujet.
- Application d’un label Gmail ou déplacement dans un dossier Outlook.
- Historique explicable des traitements et erreurs.
- Déconnexion, révocation et suppression des données.
- Exécution locale avec Docker Compose.

### Contraintes structurantes

- Frontend : React et TypeScript.
- Backend : ASP.NET Core, C# et API REST.
- Traitement asynchrone : Worker .NET.
- Persistance : PostgreSQL avec migrations.
- Identité : Keycloak via OpenID Connect.
- File de messages : RabbitMQ est retenu pour découpler les webhooks du
  traitement. Sans queue, les garanties de reprise et de non-perte demandées
  seraient difficiles à tenir proprement.
- Aucun corps de mail ni pièce jointe en base ou dans RabbitMQ.
- Tokens OAuth chiffrés au repos et absents des logs.
- Cloisonnement strict par organisation.

### Points incomplets ou ambigus

Ces sujets doivent être transformés en décisions explicites avant leur
implémentation :

1. Portée des projets : organisation entière ou projets personnels.
2. Rattachement des comptes mail : utilisateur, organisation, ou les deux.
3. Politique en cas de plusieurs correspondances.
4. Archivage Gmail : conserver `INBOX` ou le retirer après labellisation.
5. Rétention des sujets et de l’historique.
6. Politique de suppression d’un projet déjà référencé par l’historique.
7. Gestion des invitations et de l’adhésion à une organisation.
8. Cible d’hébergement et gestionnaire de secrets de production.
9. Niveau exact de révocation distante attendu chez Google et Microsoft.

### Défauts éditoriaux relevés

- La numérotation saute les sections 3.10, 4 et 10.
- L’historique est demandé dans les objectifs et critères d’acceptation, mais
  aucune page minimale dédiée n’apparaît dans la section interface.
- RabbitMQ est formulé comme optionnel alors que les garanties demandées
  impliquent pratiquement une queue durable.

## 3. Architecture cible

Le MVP doit rester un monolithe modulaire, complété par un worker séparé. Des
microservices ajouteraient des coûts de déploiement et de cohérence sans
bénéfice nécessaire à ce stade.

### Applications

- `Web` : React/TypeScript, interface utilisateur.
- `Api` : API REST, OAuth callbacks, endpoints Gmail Pub/Sub et Graph Webhooks.
- `Worker` : consommation RabbitMQ et pipeline de traitement.
- `Domain` : entités, règles métier et contrats de matching.
- `Application` : cas d’usage, autorisations et orchestration.
- `Infrastructure` : PostgreSQL, RabbitMQ, Keycloak, Gmail et Microsoft Graph.
- Projets de tests unitaires, intégration et architecture.

### Pipeline d’un événement

1. Le endpoint fournisseur valide la notification.
2. Il résout le compte connecté concerné.
3. Il crée un enregistrement d’événement avec une clé d’idempotence.
4. Il publie un message minimal dans RabbitMQ.
5. Le worker récupère les métadonnées nécessaires auprès du fournisseur.
6. Le moteur normalise le sujet et exécute les stratégies actives.
7. La politique de conflit décide de l’action.
8. L’adaptateur fournisseur applique le classement.
9. Le résultat, la justification et les erreurs sont historisés.

### Interfaces d’extension à prévoir

- `IMatchingStrategy`
- `IConflictResolutionPolicy`
- `IMailProviderAdapter`
- `IClassificationAction`
- `ITokenProtector`
- `IProcessingHistoryService`

### Modèle de données initial

- `Organization`
- `ApplicationUser`
- `OrganizationMembership`
- `MailAccount`
- `OAuthCredential`
- `Project`
- `ProjectAlias`
- `ProviderClassificationTarget`
- `MailProcessingEvent`
- `MailProcessingAttempt`
- `OrganizationSettings`
- `WebhookSubscription`

Toutes les tables métier qui portent des données client doivent contenir ou
permettre de dériver un `OrganizationId`. Les contraintes uniques doivent
inclure ce tenant lorsque nécessaire.

## 4. Décisions proposées pour démarrer

Ces choix constituent une base de travail et pourront être remplacés par des
ADR si le besoin évolue :

- Projets et règles portés par l’organisation.
- Comptes mail connectés par un utilisateur, mais visibles et administrables
  selon son rôle dans l’organisation.
- Un message ne reçoit qu’un classement de projet dans le MVP.
- En cas de plusieurs projets détectés : statut `Conflict`, aucune action
  automatique. C’est plus sûr qu’un classement arbitraire.
- Correspondance sur des limites de mots après normalisation, et non simple
  sous-chaîne brute, afin d’éviter qu’un alias court corresponde à l’intérieur
  d’un autre mot.
- Gmail : ajout d’un label, sans archivage par défaut.
- Outlook : déplacement vers un seul dossier.
- Historique sans corps ni pièce jointe ; conservation du sujet brut à éviter.
  Stocker une version masquée ou une empreinte lorsque l’explication le permet.
- Suppression logique des projets déjà référencés par l’historique.
- API décrite par OpenAPI, avec génération d’un client TypeScript.

## 5. Itérations

Chaque itération doit se terminer par du code exécutable, des tests, une
documentation mise à jour et une démonstration reproductible.

### Itération 0 — fondations et décisions

Objectif : rendre le projet reprenable et construire un socle local fiable.

- Initialiser Git et la structure du dépôt.
- Créer la solution .NET et l’application React TypeScript.
- Ajouter conventions, formatage, analyse statique et tests vides exécutables.
- Créer Docker Compose avec PostgreSQL, RabbitMQ et Keycloak.
- Définir la configuration locale et fournir `.env.example`.
- Écrire les premières décisions d’architecture.
- Consolider les ambiguïtés du cahier des charges sans en changer le sens.
- Définir la stratégie de branches et la CI minimale.

Critères de sortie :

- Un développeur lance l’infrastructure et les applications avec une procédure
  documentée.
- Build, lint et tests passent localement et en CI.
- Aucun secret réel n’est versionné.

### Itération 1 — domaine, persistance et moteur de matching

Objectif : livrer le cœur métier indépendamment des fournisseurs.

- Modéliser organisations, utilisateurs référencés, projets, alias et réglages.
- Configurer EF Core, PostgreSQL et les premières migrations.
- Implémenter les cas d’usage CRUD des projets et alias.
- Implémenter la normalisation : casse, accents, espaces, ponctuation et
  préfixes `RE`, `FW`, `FWD`, `TR`.
- Implémenter `IMatchingStrategy` et la stratégie déterministe du MVP.
- Implémenter la politique de conflit.
- Ajouter un endpoint de test manuel d’un sujet.
- Tester les ambiguïtés, accents, préfixes, projets désactivés et faux positifs.

Critères de sortie :

- Le moteur fournit un résultat explicable sans dépendre de Gmail ou Outlook.
- Les tests couvrent les principaux cas de matching.
- Les migrations recréent une base vide.

### Itération 2 — identité, organisations et autorisations

Objectif : sécuriser l’application et le cloisonnement multi-tenant.

- Configurer Keycloak et un realm de développement reproductible.
- Valider les JWT côté API.
- Synchroniser la référence utilisateur locale à la première connexion.
- Implémenter adhésion à une organisation et rôles.
- Appliquer les politiques d’autorisation côté backend.
- Ajouter des tests d’isolation entre organisations.
- Brancher l’authentification React et protéger les routes.

Critères de sortie :

- Un utilisateur authentifié ne peut lire ou modifier que son tenant autorisé.
- Les contrôles existent côté API, indépendamment du frontend.
- Les scénarios `Owner`, `Admin` et `Member` sont testés.

### Itération 3 — interface projets et paramètres

Objectif : rendre le cœur métier utilisable avant les intégrations externes.

- Créer le shell de l’application et le tableau de bord initial.
- Créer les pages liste, création et modification de projet.
- Gérer les alias et leur activation.
- Ajouter le test manuel d’un sujet avec explication du résultat.
- Ajouter les paramètres de conflit et d’archivage.
- Générer ou typer le client API TypeScript.
- Ajouter validation client et serveur ainsi que tests UI critiques.

Critères de sortie :

- Le parcours projets/alias est complet depuis l’interface.
- Les erreurs de validation et d’autorisation sont compréhensibles.

### Itération 4 — connexion Gmail et classement manuel

Objectif : valider OAuth, stockage sécurisé et adaptateur Gmail avant Pub/Sub.

- Implémenter OAuth Google avec scopes minimaux validés.
- Chiffrer les tokens et gérer leur renouvellement.
- Connecter, afficher et déconnecter un compte Gmail.
- Créer/récupérer un label et conserver son identifiant externe.
- Implémenter l’adaptateur Gmail et un classement déclenché en environnement de
  test, sans encore dépendre d’un événement Pub/Sub.
- Implémenter révocation et suppression des credentials.

Critères de sortie :

- Un compte de test Gmail peut être connecté puis révoqué.
- Un message de test reçoit le label attendu de façon idempotente.
- Aucun token n’apparaît dans les logs ou réponses API.

### Itération 5 — Gmail Pub/Sub et pipeline asynchrone

Objectif : traiter automatiquement les nouveaux messages Gmail.

- Configurer Gmail `watch`, Google Cloud Pub/Sub et renouvellement du watch.
- Valider et accuser réception des notifications.
- Publier les jobs minimaux dans RabbitMQ.
- Consommer les jobs dans le worker.
- Gérer `historyId`, récupération des messages concernés et déduplication.
- Ajouter retries bornés, backoff et dead-letter queue.
- Historiser chaque étape avec identifiant de corrélation.

Critères de sortie :

- Un nouveau mail déclenche automatiquement le classement.
- Une notification répétée ne produit pas une seconde action.
- Une erreur temporaire peut être reprise sans perte d’événement.

### Itération 6 — connexion Outlook et classement manuel

Objectif : valider OAuth Microsoft et l’adaptateur Graph.

- Implémenter OAuth Microsoft avec permissions minimales validées.
- Chiffrer, renouveler, révoquer et supprimer les tokens.
- Connecter, afficher et déconnecter un compte Microsoft.
- Créer/récupérer les dossiers et stocker leur identifiant externe.
- Déplacer un message de test de façon contrôlée et traçable.

Critères de sortie :

- Un compte Microsoft de test peut être connecté puis révoqué.
- Un message est déplacé vers le dossier attendu.
- Un retry ne provoque pas de déplacement incohérent.

### Itération 7 — Microsoft Graph Webhooks

Objectif : traiter automatiquement les nouveaux messages Microsoft 365.

- Créer, valider, renouveler et supprimer les subscriptions Graph.
- Gérer le challenge de validation du webhook.
- Valider `clientState` et résoudre le compte concerné.
- Publier les jobs dans RabbitMQ et réutiliser le pipeline commun.
- Gérer les notifications dupliquées, retardées ou hors ordre.
- Tester l’expiration et le renouvellement des subscriptions.

Critères de sortie :

- Un nouveau mail Outlook déclenche automatiquement le classement.
- Les doublons et expirations sont observables et récupérables.

### Itération 8 — historique, reprise et observabilité

Objectif : rendre le système exploitable et explicable.

- Ajouter la page d’historique et le détail d’un traitement.
- Afficher résultat, règle, projet, action, tentatives et erreur assainie.
- Permettre la reprise autorisée d’un job échoué.
- Ajouter logs structurés, métriques, health et readiness.
- Ajouter tableaux de bord minimaux sur queue, erreurs et subscriptions.
- Vérifier systématiquement l’absence de données sensibles dans les logs.

Critères de sortie :

- Chaque action ou absence d’action est explicable.
- Les jobs échoués sont visibles et relançables.
- Les dépendances défaillantes sont reflétées par les endpoints de santé.

### Itération 9 — vie privée, suppression et export

Objectif : satisfaire les exigences de maîtrise des données.

- Implémenter export de configuration.
- Implémenter suppression/désactivation d’un compte applicatif.
- Purger tokens, subscriptions et données personnelles selon la politique.
- Ajouter rétention et purge automatique de l’historique sensible.
- Documenter révocation, suppression, incident et RGPD minimal.
- Tester les parcours de suppression et leur audit.

Critères de sortie :

- Les opérations de déconnexion et suppression ont un effet vérifiable.
- La durée et la raison de conservation de chaque donnée sont documentées.

### Itération 10 — durcissement et livraison MVP

Objectif : satisfaire l’ensemble des critères d’acceptation.

- Exécuter tests unitaires, intégration, contrat et end-to-end.
- Tester concurrence, idempotence, retries et isolation tenant.
- Réaliser une revue sécurité ciblée OAuth, webhooks, secrets et logs.
- Finaliser Docker, migrations et procédures de déploiement.
- Produire un jeu de données non sensible.
- Vérifier les 20 critères d’acceptation du cahier des charges.
- Réaliser une démonstration Gmail et Outlook de bout en bout.

Critères de sortie :

- Tous les critères d’acceptation sont reliés à une preuve.
- L’installation locale et serveur est reproductible.
- Les risques résiduels sont documentés et acceptés explicitement.

## 6. Stratégie de tests

- Tests unitaires : normalisation, matching, conflits, autorisations et
  transitions d’état.
- Tests d’intégration : PostgreSQL, RabbitMQ, API, idempotence et migrations.
- Tests de contrat : réponses Gmail et Microsoft Graph simulées.
- Tests end-to-end locaux : Keycloak, interface et API.
- Tests sandbox/réels contrôlés : comptes Google et Microsoft dédiés.
- Tests d’architecture : dépendances entre couches.
- Tests de sécurité : cloisonnement tenant, scopes, fuite de secrets et
  validation des webhooks.

## 7. Risques majeurs

| Risque | Réduction prévue |
|---|---|
| Validation OAuth Google/Microsoft plus longue que prévu | Commencer les configurations cloud tôt et utiliser des comptes de test dédiés |
| Webhooks inaccessibles en local | Documenter un tunnel HTTPS de développement et prévoir des simulateurs |
| Notifications dupliquées ou incomplètes | Idempotence en base, clé unique, retries et réconciliation fournisseur |
| Fuite de tokens ou sujets dans les logs | Filtrage centralisé, revues et tests automatiques |
| Alias courts causant des faux positifs | Limites de mots, validation et détection d’ambiguïtés |
| Dérive vers des microservices prématurés | Monolithe modulaire et contrats internes stables |
| Isolement tenant incomplet | Filtrage central, politiques backend et tests négatifs systématiques |
| Expiration des watches/subscriptions | Jobs planifiés, métriques et alertes avant expiration |

## 8. Règle de conduite des prochaines sessions

Au début de chaque itération :

1. Lire `cahier_des_charges.md`, `ROADMAP.md` et `PROJECT_STATE.md`.
2. Vérifier l’état réel du dépôt et des tests.
3. Choisir une seule itération active et ses critères de sortie.
4. Mettre à jour `PROJECT_STATE.md` avant et après le travail.
5. Enregistrer les décisions durables dans un ADR.
6. Ne marquer une tâche terminée qu’après code, tests et documentation.
