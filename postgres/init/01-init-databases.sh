#!/bin/bash
set -e

# Note the <<- which allows for indented heredoc content, but the final delimiter must have no leading whitespace.
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    -- Create the 'dq' database if it doesn't exist
    SELECT 'CREATE DATABASE dq' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'dq')\gexec

    -- Create the 'fhir' database if it doesn't exist
    SELECT 'CREATE DATABASE fhir' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'fhir')\gexec
EOSQL