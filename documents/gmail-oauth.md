# Connexion Gmail OAuth

## Architecture retenue

MailAssistant utilise le flux OAuth 2.0 serveur de Google. Le navigateur est
redirigé vers Google, puis Google renvoie un code temporaire à l’API :

```text
http://localhost:5080/api/integrations/gmail/callback
```

L’API échange ce code contre des tokens, lit l’adresse du profil Gmail et ne
persiste que le refresh token chiffré. Les access tokens sont renouvelés à la
demande et restent en mémoire le temps d’un appel.

Le scope demandé est :

```text
https://www.googleapis.com/auth/gmail.modify
```

Ce scope est nécessaire pour lire le sujet d’un message et modifier ses labels.
Google le classe comme scope restreint. Une publication publique demandera la
vérification OAuth appropriée et pourra nécessiter une évaluation de sécurité.

## Configuration Google Cloud

1. Créer ou sélectionner un projet dans Google Cloud Console.
2. Activer Gmail API.
3. Configurer l’écran de consentement OAuth.
4. Tant que l’application est en mode test, ajouter le compte Gmail utilisé
   dans la liste des utilisateurs de test.
5. Créer un client OAuth de type `Web application`.
6. Ajouter exactement cette URI de redirection autorisée :

   ```text
   http://localhost:5080/api/integrations/gmail/callback
   ```

7. Copier `.env.example` vers `.env` et renseigner :

   ```dotenv
   GMAIL_CLIENT_ID=...
   GMAIL_CLIENT_SECRET=...
   GMAIL_CALLBACK_URL=http://localhost:5080/api/integrations/gmail/callback
   ```

8. Reconstruire et démarrer la stack :

   ```shell
   docker compose up --build
   ```

## Utilisation

1. Ouvrir <http://localhost:5173>.
2. Se connecter via Keycloak.
3. Sélectionner une organisation.
4. Ouvrir `Comptes email`.
5. Cliquer sur `Connecter Gmail`.
6. Accepter le consentement Google.

Un Owner ou Admin peut connecter, configurer et déconnecter un compte. Un
Member peut seulement consulter les comptes connectés.

Le classement manuel attend l’identifiant Gmail du message, pas son sujet ni
son URL complète. L’application récupère uniquement les métadonnées nécessaires,
crée ou réutilise le label du projet et retire éventuellement `INBOX` selon le
paramètre d’archivage de l’organisation.

## Protection et révocation

- Le refresh token est chiffré avec ASP.NET Data Protection.
- Les clés locales sont conservées dans le volume Docker
  `data-protection-keys`.
- Aucun token ne doit apparaître dans les logs ou réponses API.
- La déconnexion appelle l’endpoint de révocation Google puis supprime le token
  local.
- En production, les clés Data Protection devront être protégées par un
  mécanisme externe adapté à la plateforme de déploiement.

## Références officielles

- [OAuth 2.0 pour applications serveur](https://developers.google.com/identity/protocols/oauth2/web-server)
- [Scopes Gmail](https://developers.google.com/workspace/gmail/api/auth/scopes)
- [Modification des labels d’un message](https://developers.google.com/workspace/gmail/api/reference/rest/v1/users.messages/modify)
- [Bonnes pratiques OAuth](https://developers.google.com/identity/protocols/oauth2/resources/best-practices)
