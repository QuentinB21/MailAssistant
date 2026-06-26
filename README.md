# MailAssistant

Application web multi-tenant qui classe automatiquement les emails Gmail et
Microsoft 365 selon les projets détectés dans leur sujet.

Le projet entre dans l’itération 4. La feuille de route et l’état courant sont
disponibles dans :

- [`documents/ROADMAP.md`](documents/ROADMAP.md)
- [`documents/PROJECT_STATE.md`](documents/PROJECT_STATE.md)

## Prérequis

- Docker avec Docker Compose.

.NET et Node.js ne sont nécessaires sur la machine hôte que pour le
développement hors Docker. Les images de la stack fournissent leurs propres
runtimes.

## Démarrage local

Démarrer toute la stack :

```shell
docker compose up
```

Au premier lancement, Docker construit automatiquement les images. Après une
modification du code, forcer leur reconstruction avec :

```shell
docker compose up --build
```

La stack contient le frontend Nginx, l’API .NET, le Worker .NET, PostgreSQL,
RabbitMQ et Keycloak. L’API applique automatiquement les migrations avant de
démarrer.

Ouvrir ensuite l’application sur <http://localhost:5173>.

Services exposés :

- application : <http://localhost:5173>
- API : <http://localhost:5080>
- santé API : <http://localhost:5080/health>
- Keycloak : <http://localhost:8080>
- RabbitMQ Management : <http://localhost:15672>
- PostgreSQL : `localhost:5432`

Arrêter les conteneurs :

```shell
docker compose down
```

## Commandes principales

```shell
# Stack complète reconstruite
docker compose up --build

# Logs de tous les services
docker compose logs --follow

# Développement local avec rechargement à chaud
npm run dev:local

# Validation complète backend, frontend et Docker Compose
npm run verify

# Scénario réel Keycloak/PostgreSQL/API
npm run test:auth
```

Les commandes `npm` sont réservées au développement et à la CI. L’exécution
normale de l’application ne dépend que de Docker Compose.

## Authentification locale

La connexion utilise le realm Keycloak `mailassistant`. Les comptes locaux,
les rôles et la séparation entre administration Keycloak et utilisateurs
applicatifs sont documentés dans
[`documents/authentication.md`](documents/authentication.md).

La configuration Google Cloud et le flux Gmail sont documentés dans
[`documents/gmail-oauth.md`](documents/gmail-oauth.md).

Toutes les routes métier nécessitent un JWT Keycloak valide :

- `GET /api/me`
- CRUD `/api/organizations`
- CRUD `/api/organizations/{organizationId}/projects`
- CRUD `/api/organizations/{organizationId}/projects/{projectId}/aliases`
- `POST /api/organizations/{organizationId}/matching-tests`
- paramètres `/api/organizations/{organizationId}/settings`
- comptes Gmail `/api/organizations/{organizationId}/mail-accounts/gmail`
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
