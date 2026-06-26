# ADR 0006 — OAuth Gmail serveur et protection des tokens

## Statut

Acceptée.

## Contexte

MailAssistant doit agir sur Gmail lorsque l’utilisateur n’est plus présent,
sans confondre l’identité Keycloak avec l’autorisation Google. Le système doit
supporter plusieurs comptes par organisation, renouveler les access tokens et
ne jamais stocker de token en clair.

## Décision

- Utiliser le flux OAuth 2.0 serveur avec accès hors ligne.
- Demander uniquement `gmail.modify`, nécessaire pour lire les métadonnées et
  appliquer ou retirer des labels.
- Porter les comptes Gmail au niveau organisation et conserver l’utilisateur
  ayant réalisé la connexion.
- Autoriser Owner et Admin à administrer les comptes ; Member dispose d’un
  accès en lecture.
- Persister uniquement le refresh token, chiffré avec ASP.NET Data Protection.
- Ne pas persister les access tokens.
- Conserver une correspondance durable entre un projet et l’identifiant du
  label Gmail pour chaque compte.
- Utiliser un adaptateur `IGmailGateway` afin d’isoler OAuth et Gmail API du
  domaine et des services applicatifs.

## Conséquences

- Le volume des clés Data Protection doit être sauvegardé avec la base ; sa
  perte rend les refresh tokens indéchiffrables.
- La production devra utiliser une protection de clés adaptée à son
  environnement.
- `gmail.modify` est un scope restreint et entraîne des obligations de
  vérification Google avant diffusion publique.
- Le flux réel ne peut être validé sans client OAuth Google et compte Gmail de
  test.
