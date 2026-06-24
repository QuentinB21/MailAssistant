# MailAssistant

Application web multi-tenant qui classe automatiquement les emails Gmail et
Microsoft 365 selon les projets détectés dans leur sujet.

Le projet entre dans l’itération 3. La feuille de route et l’état courant sont
disponibles dans :

- [`documents/ROADMAP.md`](documents/ROADMAP.md)
- [`documents/PROJECT_STATE.md`](documents/PROJECT_STATE.md)

## Prérequis

- .NET SDK 10.0.100 ou correctif compatible ;
- Node.js 22.12 ou plus récent ;
- Docker avec Docker Compose.

Les versions structurantes sont verrouillées par `global.json`, `.nvmrc` et les
fichiers de packages.

## Démarrage local

Installer les dépendances frontend une première fois :

```shell
npm ci --prefix frontend
```

Puis démarrer l’infrastructure, appliquer les migrations et lancer l’API, le
worker et le frontend :

```shell
npm run dev
```

Services exposés :

- application : <http://localhost:5173>
- API : <http://localhost:5080>
- santé API : <http://localhost:5080/health>
- Keycloak : <http://localhost:8080>
- RabbitMQ Management : <http://localhost:15672>
- PostgreSQL : `localhost:5432`

Arrêter l’application avec `Ctrl+C`. Pour arrêter aussi l’infrastructure :

```shell
npm run infra:down
```

## Commandes principales

```shell
# Validation complète backend, frontend et Docker Compose
npm run verify

# Scénario réel Keycloak/PostgreSQL/API
npm run test:auth

# Infrastructure uniquement
npm run infra:up

# Application des migrations
npm run db:update
```

Ces commandes sont pilotées par Node.js et fonctionnent sous Windows, Linux et
macOS. Elles sont également utilisées par la CI lorsque pertinent.

## Authentification locale

La connexion utilise le realm Keycloak `mailassistant`. Les comptes locaux,
les rôles et la séparation entre administration Keycloak et utilisateurs
applicatifs sont documentés dans
[`documents/authentication.md`](documents/authentication.md).

Toutes les routes métier nécessitent un JWT Keycloak valide :

- `GET /api/me`
- CRUD `/api/organizations`
- CRUD `/api/organizations/{organizationId}/projects`
- CRUD `/api/organizations/{organizationId}/projects/{projectId}/aliases`
- `POST /api/organizations/{organizationId}/matching-tests`
- gestion `/api/organizations/{organizationId}/members`

## Structure

```text
src/
  MailAssistant.Api/             API REST et authentification
  MailAssistant.Application/     cas d’usage et autorisations
  MailAssistant.Domain/          modèle et règles métier
  MailAssistant.Infrastructure/  PostgreSQL et intégrations externes
  MailAssistant.Worker/          traitements asynchrones
frontend/                        application React TypeScript
tests/                           tests automatisés .NET
infrastructure/                  configuration locale reproductible
documents/                       spécifications, suivi, guides et ADR
tools/                           orchestration multiplateforme
```

Documentation complémentaire :

- [`documents/data-model.md`](documents/data-model.md)
- [`documents/matching.md`](documents/matching.md)
- [`documents/quality-review.md`](documents/quality-review.md)
- [`documents/adr`](documents/adr)

## Configuration et secrets

- `.env.example` et `frontend/.env.example` documentent les variables.
- Les valeurs par défaut sont strictement locales.
- Aucun token OAuth ou secret réel ne doit être versionné.
- Les comptes et mots de passe du realm importé sont des données de test
  publiques, interdites en production.

## Conventions

- Branches courtes depuis `main`.
- Une fonctionnalité inclut code, tests et documentation.
- Les décisions durables sont enregistrées dans `documents/adr`.
- `documents/PROJECT_STATE.md` est mis à jour à chaque session significative.
- Une revue cleanup/refactoring est planifiée après chaque groupe de trois
  itérations.
