# MailAssistant — état du projet

Dernière mise à jour : 2026-06-24

Ce fichier est la mémoire opérationnelle du projet. Il doit être mis à jour à
chaque session significative. La vision et le découpage sont dans
`ROADMAP.md`.

## État global

- Phase actuelle : sécurisation de l’application.
- Itération active : itération 2 — identité, organisations et autorisations.
- Dernière itération terminée : itération 1 — domaine, persistance et matching.
- Statut du MVP : cœur métier et persistance opérationnels, authentification à réaliser.
- Dépôt Git : branche `main` reliée à `origin/main`.
- Code applicatif : API CRUD projets/alias, matching, worker et frontend présents.
- Tests : 16 tests .NET et 1 test Vitest validés.
- Infrastructure locale : PostgreSQL, RabbitMQ et Keycloak validés avec Docker
  Compose.

## Réalisé

- [x] Analyse initiale du cahier des charges.
- [x] Identification du périmètre, des ambiguïtés et des risques.
- [x] Proposition d’une architecture de monolithe modulaire avec worker.
- [x] Découpage du MVP en itérations avec critères de sortie.
- [x] Création du registre d’avancement persistant.
- [x] Initialisation du dépôt Git sur `main`.
- [x] Création de la solution .NET 10 et de ses modules.
- [x] Création du frontend React/TypeScript avec Vite.
- [x] Ajout des premiers tests xUnit et Vitest.
- [x] Ajout de PostgreSQL, RabbitMQ et Keycloak dans Docker Compose.
- [x] Ajout du realm Keycloak de développement.
- [x] Ajout des conventions, du verrouillage des versions et de la CI.
- [x] Ajout du README de lancement local et des premiers ADR.
- [x] Validation du build, des tests, du lint et de l’audit des dépendances.
- [x] Validation des trois conteneurs Docker à l’état `healthy`.
- [x] Validation de l’endpoint API `/health` et du démarrage du worker.
- [x] Modélisation des organisations, réglages, projets et alias.
- [x] Ajout d’EF Core 10 et du provider PostgreSQL.
- [x] Création et application de la migration initiale.
- [x] Implémentation des repositories et de l’unité de travail.
- [x] Implémentation de la normalisation des sujets.
- [x] Implémentation de la stratégie de matching déterministe.
- [x] Implémentation de la politique de conflit sans action.
- [x] Exposition des API CRUD organisations, projets et alias.
- [x] Exposition de l’API de test manuel d’un sujet.
- [x] Validation du parcours API/PostgreSQL de bout en bout.
- [x] Documentation du modèle de données et du moteur de matching.

## Prochaine étape recommandée

Exécuter l’itération 2 dans cet ordre :

1. Configurer la validation JWT Keycloak dans l’API.
2. Modéliser les utilisateurs locaux et appartenances aux organisations.
3. Synchroniser l’utilisateur à sa première connexion.
4. Implémenter les rôles `Owner`, `Admin` et `Member`.
5. Remplacer la confiance dans `organizationId` par une autorisation tenant.
6. Ajouter les tests négatifs d’isolation entre organisations.
7. Brancher l’authentification React et protéger les routes.

## Décisions provisoires

Les décisions ci-dessous sont proposées mais pas encore validées par le
propriétaire du projet :

| ID | Décision | Statut |
|---|---|---|
| D-001 | Utiliser un monolithe modulaire avec API et worker séparés | Acceptée, ADR 0001 |
| D-002 | Intégrer RabbitMQ dès le MVP | Acceptée, ADR 0002 |
| D-003 | Porter les projets et règles au niveau organisation | Acceptée, ADR 0004 |
| D-004 | Classer un message dans un seul projet pour le MVP | Acceptée, ADR 0004 |
| D-005 | En cas de correspondances multiples, ne rien classer et tracer un conflit | Acceptée, ADR 0004 |
| D-006 | Gmail ajoute un label sans archiver par défaut | Proposée |
| D-007 | Générer un client TypeScript depuis OpenAPI | Proposée |
| D-008 | Éviter de conserver le sujet brut dans l’historique | Proposée |
| D-009 | Cibler .NET 10 pour le backend | Acceptée, remplace la décision initiale .NET 8 |
| D-010 | Exiger Node.js 22.12+ et utiliser React avec Vite | Acceptée |

Lorsqu’une décision devient stable, créer un fichier dans
`docs/adr/NNNN-titre.md`, puis remplacer son statut par `Acceptée`.

## Questions ouvertes

- [x] Les projets sont exclusivement partagés par organisation.
- [ ] Un utilisateur peut-il appartenir à plusieurs organisations ?
- [ ] Qui peut connecter et administrer un compte mail ?
- [ ] Faut-il autoriser plusieurs labels Gmail en cas de plusieurs matchs ?
- [ ] Quelle durée de conservation pour l’historique et les sujets ?
- [ ] Quelle cible de déploiement est prévue pour le MVP ?
- [ ] Quel coffre de secrets sera utilisé en production ?
- [ ] Faut-il une invitation par email pour rejoindre une organisation ?
- [ ] Quelle politique de reprise manuelle des traitements échoués ?

## Bloquants

Aucun bloquant pour démarrer l’itération 2.

Node.js 22.23.1 est installé via WinGet. Les terminaux ouverts avant
l’installation doivent être redémarrés pour récupérer le nouveau `PATH`.

Les intégrations réelles demanderont ultérieurement :

- un projet Google Cloud avec OAuth, Gmail API et Pub/Sub ;
- une application Microsoft Entra ID avec Microsoft Graph ;
- des comptes Gmail et Microsoft 365 de test ;
- une URL HTTPS publiquement accessible pour tester les webhooks ;
- les choix de rétention et de cible de déploiement.

## Journal des sessions

### 2026-06-24 — cadrage initial

- Lecture du cahier des charges.
- Constat : le répertoire ne contient que `cahier_des_charges.md`.
- Constat : le répertoire n’est pas encore un dépôt Git.
- Création de `ROADMAP.md`.
- Création de `PROJECT_STATE.md`.
- Aucun code produit et aucune infrastructure modifiée.

### 2026-06-24 — itération 0

- Itération : 0 — fondations et décisions.
- Objectif : obtenir un socle local reproductible, compilable et testé.
- Réalisé : dépôt Git, solution .NET 10 modulaire, API, worker, frontend
  React/TypeScript, tests, Docker Compose, Keycloak, CI, conventions et README.
- Tests exécutés : build .NET sans avertissement, 1 test xUnit, lint frontend,
  1 test Vitest, build frontend, `npm audit` sans vulnérabilité, validation
  Docker Compose, santé API HTTP 200 et démarrage du worker.
- Infrastructure : PostgreSQL 16.8, RabbitMQ 4.0.7 et Keycloak 26.1.4 démarrés
  et validés `healthy`.
- Décisions prises : ADR 0001 monolithe modulaire ; ADR 0002 RabbitMQ dans le
  MVP ; .NET 10 ; Node.js 22.12 minimum.
- Risques ou dettes : aucune fonctionnalité métier ni persistance EF Core
  encore implémentée ; Node.js système doit être mis à niveau.
- Prochaine action exacte : modéliser le domaine de l’itération 1 et ajouter
  EF Core avec la première migration.

### 2026-06-24 — passage à .NET 10

- Décision utilisateur : remplacer la cible .NET 8 par .NET 10.
- SDK verrouillé : 10.0.100.
- Framework cible : `net10.0`.
- Packages `Microsoft.Extensions.*` alignés sur la version 10.0.9.

### 2026-06-24 — itération 1

- Itération : 1 — domaine, persistance et moteur de matching.
- Objectif : livrer le cœur métier indépendamment de Gmail et Outlook.
- Réalisé : organisations, réglages, projets, alias, EF Core/PostgreSQL,
  migration initiale, repositories, CRUD REST et test manuel de sujet.
- Matching : suppression des préfixes de réponse/transfert, casse, accents,
  ponctuation et espaces ; correspondance sur limites de termes ; projets et
  alias désactivés ignorés ; conflits sans action.
- Tests exécutés : build .NET sans avertissement, 16 tests xUnit, migration
  sans changement de modèle en attente, migration appliquée sur PostgreSQL,
  parcours API réel `Matched` et `Conflict`, lint/test/build/audit frontend.
- Décisions prises : projets par organisation, un seul projet sélectionnable,
  conflit sans action automatique, ADR 0004.
- Fichiers principaux : domaines `Organizations`, `Projects`, `Matching`,
  services applicatifs, `MailAssistantDbContext`, migrations et endpoints API.
- Risques ou dettes : les routes contiennent encore un `organizationId` fourni
  par le client ; l’itération 2 doit impérativement vérifier l’appartenance via
  Keycloak avant toute exposition réelle.
- Prochaine action exacte : valider les JWT Keycloak et introduire les
  appartenances utilisateur/organisation.

## Modèle de mise à jour pour la prochaine session

Copier et compléter ce bloc dans le journal :

```text
### AAAA-MM-JJ — titre de la session

- Itération :
- Objectif :
- Réalisé :
- Tests exécutés :
- Décisions prises :
- Fichiers principaux modifiés :
- Risques ou dettes :
- Prochaine action exacte :
```
