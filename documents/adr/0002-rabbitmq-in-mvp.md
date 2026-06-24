# ADR 0002 — RabbitMQ intégré dès le MVP

- Statut : accepté
- Date : 2026-06-24

## Contexte

Les notifications Gmail et Microsoft doivent être acquittées rapidement,
traitées sans perte et reprises en cas d’erreur temporaire.

## Décision

RabbitMQ découple la réception des notifications du traitement métier dès le
MVP. Les messages de queue ne contiendront que les identifiants et métadonnées
techniques nécessaires, jamais le contenu complet d’un email.

## Conséquences

- Les webhooks restent courts et fiables.
- Retries et dead-letter queues pourront être ajoutés au worker.
- Une dépendance opérationnelle supplémentaire est assumée et supervisée.
