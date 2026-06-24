# MailAssistant

Application de classement automatique des emails Gmail et Microsoft 365 selon
les projets détectés dans leur sujet.

Le projet est actuellement dans l’itération 0. La feuille de route est décrite
dans [`ROADMAP.md`](ROADMAP.md) et l’état courant dans
[`PROJECT_STATE.md`](PROJECT_STATE.md).

## Prérequis

- .NET SDK 10.0.100 ou version corrective compatible ;
- Node.js 22.12 ou plus récent ;
- Docker avec Docker Compose ;
- PowerShell pour le script de vérification fourni.

Les versions de SDK et de dépendances sont verrouillées dans le dépôt.

## Démarrage local

Copier la configuration locale :

```powershell
Copy-Item .env.example .env
```

Les valeurs par défaut sont uniquement destinées au développement local. Ne
jamais les réutiliser en production.

Démarrer PostgreSQL, RabbitMQ et Keycloak :

```powershell
docker compose up -d
```

Appliquer les migrations PostgreSQL :

```powershell
dotnet tool restore
dotnet ef database update `
  --project src/MailAssistant.Infrastructure `
  --startup-project src/MailAssistant.Api
```

Services exposés :

- Keycloak : <http://localhost:8080>
- RabbitMQ Management : <http://localhost:15672>
- PostgreSQL : `localhost:5432`

Démarrer l’API :

```powershell
dotnet run --project src/MailAssistant.Api
```

L’API répond sur <http://localhost:5080> et son endpoint de santé sur
<http://localhost:5080/health>.

Démarrer le worker dans un second terminal :

```powershell
dotnet run --project src/MailAssistant.Worker
```

Démarrer le frontend :

```powershell
Set-Location frontend
npm ci
npm run dev
```

Le frontend répond sur <http://localhost:5173>.

## API du cœur métier

L’itération 1 expose les routes suivantes :

- `POST /api/organizations`
- `GET /api/organizations`
- `GET /api/organizations/{organizationId}`
- CRUD `/api/organizations/{organizationId}/projects`
- CRUD `/api/organizations/{organizationId}/projects/{projectId}/aliases`
- `POST /api/organizations/{organizationId}/matching-tests`

Exemple de test manuel :

```json
{
  "subject": "RE: Compte rendu Projet Apollo"
}
```

Le résultat vaut `Matched`, `NoMatch` ou `Conflict`. En cas de conflit, aucune
sélection automatique n’est effectuée.

## Vérification complète

```powershell
./scripts/verify.ps1
```

Ce script restaure, compile et teste le backend, puis vérifie le lint, les
tests et le build frontend ainsi que la syntaxe Docker Compose.

## Structure

```text
src/
  MailAssistant.Api/             API REST et webhooks
  MailAssistant.Application/     cas d’usage et orchestration
  MailAssistant.Domain/          modèle et règles métier
  MailAssistant.Infrastructure/  persistance et fournisseurs externes
  MailAssistant.Worker/          traitements asynchrones
frontend/                        application React TypeScript
tests/                           tests automatisés .NET
infrastructure/                  configuration locale reproductible
docs/adr/                        décisions d’architecture
```

Le modèle de données est documenté dans
[`docs/data-model.md`](docs/data-model.md) et le moteur dans
[`docs/matching.md`](docs/matching.md).

## Configuration et secrets

- `.env.example` documente les variables locales.
- `.env` est ignoré par Git.
- Aucun token OAuth ou secret réel ne doit être ajouté au dépôt.
- Le realm Keycloak fourni ne contient ni utilisateur ni mot de passe métier.

## Conventions

- Branches courtes depuis `main`.
- Une fonctionnalité inclut ses tests et sa documentation.
- Les décisions durables sont enregistrées dans `docs/adr`.
- `PROJECT_STATE.md` est mis à jour à chaque session significative.
