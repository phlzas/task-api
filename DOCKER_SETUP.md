# Docker & PostgreSQL Setup Guide

Complete guide to running the Task API with PostgreSQL in Docker.

## Quick Start

```bash
# Start everything (PostgreSQL + Task API)
docker compose up

# In another terminal, test the API
curl http://localhost:5241/tasks

# Stop everything
docker compose down
```

That's it! The entire stack starts with one command.

## Prerequisites

### Docker Desktop (Includes Docker Compose)

- **Windows**: [Download Docker Desktop for Windows](https://www.docker.com/products/docker-desktop)
- **Mac**: [Download Docker Desktop for Mac](https://www.docker.com/products/docker-desktop)
- **Linux**: [Install Docker Engine + Compose](https://docs.docker.com/engine/install/)

Verify installation:

```bash
docker --version
docker compose --version
```

## What Runs in Docker

The `docker-compose.yml` file defines two containers:

### 1. PostgreSQL Database

```yaml
postgres:
  image: postgres:16-alpine  # Lightweight PostgreSQL
  environment:
    POSTGRES_USER: postgres
    POSTGRES_PASSWORD: postgres123
    POSTGRES_DB: tasks_db
  volumes:
    - postgres_data:/var/lib/postgresql/data  # Persistent storage
  healthcheck:  # Ensures database is ready before app starts
    test: ["CMD-SHELL", "pg_isready -U postgres"]
    interval: 10s
    timeout: 5s
    retries: 5
```

**Key Features**:
- Lightweight Alpine Linux image (~160MB)
- Persistent volume: data survives container restarts
- Health checks: app waits for database to be ready
- Environment variables: connection credentials

### 2. Task API Application

```yaml
app:
  build: .  # Builds from Dockerfile
  depends_on:
    postgres:
      condition: service_healthy  # Waits for database
  environment:
    - DATABASE_HOST=postgres
    - DATABASE_PORT=5432
    - DATABASE_NAME=tasks_db
    - DATABASE_USER=postgres
    - DATABASE_PASSWORD=postgres123
```

**Key Features**:
- Multi-stage Docker build (optimized image size)
- Automatic migrations on startup
- Depends on PostgreSQL health check
- Environment variables passed to .NET app

## Configuration: .env Files

### .env.example (Committed to Git)

Template showing all configuration options:

```env
DATABASE_HOST=postgres
DATABASE_PORT=5432
DATABASE_NAME=tasks_db
DATABASE_USER=postgres
DATABASE_PASSWORD=postgres123
ASPNETCORE_ENVIRONMENT=Development
```

**Always commit this** so others know what to configure.

### .env (Gitignored)

Your actual secrets:

```env
DATABASE_HOST=postgres
DATABASE_PORT=5432
DATABASE_NAME=tasks_db
DATABASE_USER=postgres
DATABASE_PASSWORD=postgres123  # Never commit this!
ASPNETCORE_ENVIRONMENT=Development
```

**Never commit .env** — it contains passwords.

## Running the Stack

### Start Everything

```bash
# Foreground (see logs in terminal)
docker compose up

# Background (runs silently)
docker compose up -d
```

Expected output:

```
Creating task_api_postgres ... done
Creating task_api_app ... done
Attaching to task_api_postgres, task_api_app
task_api_postgres | 2024-10-01 00:00:00.000 UTC [1] LOG:  database system is ready to accept connections
task_api_app | info: Microsoft.EntityFrameworkCore.Database.Command[20101]
task_api_app |       CREATE TABLE IF NOT EXISTS "TaskItems" ...
task_api_app | Listening on http://+:5241
```

### Stop Everything

```bash
# Stop containers (data persists in volumes)
docker compose down

# Stop and remove volumes (destroys data)
docker compose down -v
```

### View Logs

```bash
# All services
docker compose logs

# Specific service
docker compose logs postgres
docker compose logs app

# Follow logs in real-time
docker compose logs -f app

# View all logs with timestamps
docker compose logs --timestamps
```

## Testing Persistence

Prove that data survives restarts:

```bash
# 1. Start everything
docker compose up

# 2. Create a task (in another terminal)
curl -X POST http://localhost:5241/tasks \
  -H "Content-Type: application/json" \
  -d '{"title":"Persistence Test"}'

# 3. List tasks
curl http://localhost:5241/tasks
# Should show your task

# 4. Stop everything (Ctrl+C or docker compose down)

# 5. Start again
docker compose up

# 6. List tasks again
curl http://localhost:5241/tasks
# Task is still there! Data persisted in Docker volume
```

## Database Access

### From Container

```bash
# Enter PostgreSQL CLI
docker compose exec postgres psql -U postgres -d tasks_db

# Inside psql:
SELECT * FROM "TaskItems";
UPDATE "TaskItems" SET "IsCompleted" = true WHERE "Id" = 1;
\dt          # List tables
\d "TaskItems"  # Describe table
\q           # Quit
```

### From Host (if PostgreSQL installed locally)

```bash
psql -h localhost -U postgres -d tasks_db -p 5432
# Password: postgres123
```

### Using GUI Tool

- **Windows/Mac**: [pgAdmin 4](https://www.pgadmin.org/)
- **All platforms**: [DBeaver Community](https://dbeaver.io/)

Connection details:
- Host: `localhost`
- Port: `5432`
- Database: `tasks_db`
- User: `postgres`
- Password: `postgres123`

## Useful Commands Reference

```bash
# Container management
docker compose up              # Start all services
docker compose up -d           # Start in background
docker compose down            # Stop all services
docker compose down -v         # Stop and remove volumes (destroys data)

# Viewing
docker compose logs            # View all logs
docker compose logs -f app     # Follow app logs
docker compose ps              # List running containers

# Database access
docker compose exec postgres psql -U postgres -d tasks_db

# Rebuild after code changes
docker compose build           # Rebuild images
docker compose up --build      # Rebuild and start

# Container shell
docker compose exec app sh     # Enter app container shell
docker compose exec postgres bash  # PostgreSQL container shell

# Volumes
docker volume ls               # List all volumes
docker volume inspect postgres_data  # Show volume details
docker volume rm postgres_data # Delete volume (destroy data)
```

## SQL Examples

Connect to database (use one method above) and run:

```sql
-- List all tasks
SELECT * FROM "TaskItems";

-- List only incomplete tasks
SELECT * FROM "TaskItems" WHERE "IsCompleted" = false;

-- Count tasks
SELECT COUNT(*) FROM "TaskItems";

-- Count completed tasks
SELECT COUNT(*) FROM "TaskItems" WHERE "IsCompleted" = true;

-- Show execution plan (good for query optimization)
EXPLAIN ANALYZE SELECT * FROM "TaskItems" WHERE "IsCompleted" = true;

-- Create index (speeds up queries)
CREATE INDEX idx_task_items_is_completed 
  ON "TaskItems"("IsCompleted");

-- Update a task
UPDATE "TaskItems" 
  SET "IsCompleted" = true 
  WHERE "Id" = 1;

-- Delete all completed tasks
DELETE FROM "TaskItems" 
  WHERE "IsCompleted" = true;

-- See all indexes
\di

-- See table structure
\d "TaskItems"
```

## Troubleshooting

### "Port 5432 already in use"

Another PostgreSQL or container is using the port:

```bash
# Option 1: Stop the conflicting container
docker ps  # Find the container
docker stop <container_id>

# Option 2: Use a different port in docker-compose.yml
# Change: ports: - "5433:5432"
```

### "App can't connect to database"

```bash
# Check if PostgreSQL is healthy
docker compose ps

# View PostgreSQL logs
docker compose logs postgres

# Wait for health check to pass
docker compose logs postgres | grep "ready to accept"
```

### "Permission denied" when starting containers

Use `sudo` on Linux:

```bash
sudo docker compose up
```

### "Cannot connect from host"

If you're trying to connect from outside Docker:

```bash
# This works (inside docker network)
docker compose exec postgres psql -U postgres -d tasks_db

# This also works (host machine with Docker)
psql -h localhost -U postgres -d tasks_db -p 5432
```

### Data disappeared after restart

Check if you ran `docker compose down -v` (that deletes volumes):

```bash
# View all volumes
docker volume ls | grep postgres_data

# If volume exists, container restarts will preserve data
# If volume is gone, you need to recreate it
docker compose up  # Creates new volume with sample data
```

## Performance: Index Optimization

### Before Index

```bash
# Connect to database
docker compose exec postgres psql -U postgres -d tasks_db

# Run query WITHOUT index
EXPLAIN ANALYZE SELECT * FROM "TaskItems" WHERE "IsCompleted" = true;
```

You'll see it scans the entire table (Seq Scan).

### Create Index

```sql
CREATE INDEX idx_task_is_completed ON "TaskItems"("IsCompleted");
```

### After Index

Run the same query:

```sql
EXPLAIN ANALYZE SELECT * FROM "TaskItems" WHERE "IsCompleted" = true;
```

Now it uses the index (Index Scan), much faster on large tables.

## Docker Networking

The containers communicate via a custom bridge network:

```yaml
networks:
  task_api_network:
    driver: bridge
```

- `postgres` hostname resolves to PostgreSQL container
- `app` connects using `DATABASE_HOST=postgres`
- Both isolated from other Docker containers

## Volumes & Persistence

The `postgres_data` volume stores PostgreSQL files:

```bash
# View volume location
docker volume inspect postgres_data
# Shows: "Mountpoint": "/var/lib/docker/volumes/postgres_data/_data"

# This survives:
# - Container stop/start
# - docker compose down
# - Application restarts

# This destroys data:
# - docker volume rm postgres_data
# - docker compose down -v
```

## Security Notes

**Development Only**: The credentials are hardcoded for simplicity.

**Production**:
- Use strong passwords
- Store in AWS Secrets Manager, Azure Key Vault, etc.
- Use SSL/TLS for database connections
- Don't commit credentials to git
- Use role-based access control (RBAC)

Example production `.env`:

```env
DATABASE_HOST=prod-db.region.rds.amazonaws.com
DATABASE_PORT=5432
DATABASE_NAME=production_db
DATABASE_USER=app_user
DATABASE_PASSWORD=<SecurePasswordFromSecretManager>
ASPNETCORE_ENVIRONMENT=Production
```

## Next Steps

1. ? Run `docker compose up`
2. ? Create a task with curl
3. ? Verify persistence by restarting
4. ? Connect to database with `docker compose exec postgres psql ...`
5. ? Run SQL queries and see how indexes affect performance
6. ? Read Docker best practices documentation

## Resources

- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Entity Framework Core with PostgreSQL](https://www.npgsql.org/efcore/)
- [Docker Networking](https://docs.docker.com/network/)
- [Docker Volumes](https://docs.docker.com/storage/volumes/)

