# ADR 0001 — monolithe modulaire avec worker séparé

- Statut : accepté
- Date : 2026-06-24

## Contexte

Le MVP combine une API, un domaine métier, deux fournisseurs externes et des
traitements asynchrones. Le cahier des charges exige une séparation claire des
responsabilités, sans imposer de microservices.

## Décision

Le backend est construit comme un monolithe modulaire composé de projets
`Domain`, `Application`, `Infrastructure` et `Api`. Un processus `Worker`
séparé réutilise les mêmes modules applicatifs pour les traitements RabbitMQ.

## Conséquences

- Les frontières internes restent explicites et testables.
- Le déploiement demeure simple pour le MVP.
- L’API et le worker peuvent évoluer et être dimensionnés séparément.
- Une extraction future reste possible si un besoin mesuré la justifie.

