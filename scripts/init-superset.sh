#!/bin/bash
set -e

echo "Initializing Superset..."
docker exec -it dq-superset superset db upgrade
docker exec -it dq-superset superset init
docker exec -it dq-superset superset fab create-admin \
    --username admin \
    --firstname Admin \
    --lastname User \
    --email admin@superset.com \
    --password admin

echo "Restarting Superset..."
docker compose restart superset