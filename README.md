# Task API

FlyRank Internship — Backend AI Engineering — W2 · A1

A small REST API for managing a to-do list. Create, read, update and delete
tasks; the data is persisted in PostgreSQL running in Docker, so it survives
restarts and scales with your application.

Built with **ASP.NET Core on .NET 8**, using MVC controllers and Entity Framework Core with PostgreSQL.

## Requirements

- [Docker Desktop](https://www.docker.com/products/docker-desktop) (includes Docker Compose)
- Optional: [.NET SDK 8.0](https://dotnet.microsoft.com/download) for local development without Docker

## Run

### With Docker (Recommended)

One command starts everything:

```bash
docker compose up
```

This starts:
- **PostgreSQL 16** container with persistent volume
- **Task API** container with automatic migrations

The API is available at `http://localhost:5241` and Swagger UI at `http://localhost:5241/docs`.

### Without Docker (Local Development)

Requires PostgreSQL running locally. Create `.env` from `.env.example` and run:

```bash
dotnet run --project tasks
```

The server starts on `http://localhost:5241`. Swagger UI is at
**<http://localhost:5241/docs>**.

> If `dotnet run` picks the HTTPS profile instead, either use the
> `https://localhost:7212` base URL, or add `--urls http://localhost:5241` to
> force plain HTTP. The documented commands below assume
> `http://localhost:5241`.

## Docker & PostgreSQL

The entire stack is defined in `docker-compose.yml`:

```yaml
version: '3.8'
services:
  postgres:
    image: postgres:16-alpine
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres123
      POSTGRES_DB: tasks_db
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

  app:
    build: .
    depends_on:
      postgres:
        condition: service_healthy
    environment:
      - DATABASE_HOST=postgres
      - DATABASE_PORT=5432
      - DATABASE_NAME=tasks_db
      - DATABASE_USER=postgres
      - DATABASE_PASSWORD=postgres123

volumes:
  postgres_data:
    driver: local
```

### Configuration

Connection details are in `.env` (gitignored; `.env.example` committed):

```env
DATABASE_HOST=postgres
DATABASE_PORT=5432
DATABASE_NAME=tasks_db
DATABASE_USER=postgres
DATABASE_PASSWORD=postgres123
ASPNETCORE_ENVIRONMENT=Development
```

**Never commit `.env`** — it contains passwords. Always commit `.env.example` with safe defaults.

### Proving Persistence

The database persists across app restarts (data saved in Docker volume):

```bash
# Start the stack
docker compose up

# In another terminal, create a task
curl -X POST http://localhost:5241/tasks \
  -H "Content-Type: application/json" \
  -d '{"title":"Test persistence"}'

# Stop the containers (Ctrl+C)

# Restart everything — your task is still there
docker compose up

# List tasks — they persist
curl http://localhost:5241/tasks
```

The PostgreSQL data is stored in a Docker volume (`postgres_data`) that survives container restarts.

### Database Schema

The `TaskItems` table is auto-created by EF Core migrations with:
- `Id` (SERIAL PRIMARY KEY, auto-increment)
- `Title` (VARCHAR, required)
- `IsCompleted` (BOOLEAN, defaults to false)
- Indexes on `IsCompleted` and `Title` for query performance

### Useful Docker Commands

```bash
# Start the stack (foreground, see logs)
docker compose up

# Start in background
docker compose up -d

# Stop everything (data persists in volumes)
docker compose down

# View logs
docker compose logs -f app
docker compose logs -f postgres

# Enter the PostgreSQL database
docker compose exec postgres psql -U postgres -d tasks_db

# Clean up (remove volumes — loses data)
docker compose down -v

# View all volumes
docker volume ls | grep postgres_data
```

### SQL Examples in PostgreSQL

Once inside `psql` (run `docker compose exec postgres psql -U postgres -d tasks_db`):

```sql
-- List all tasks
SELECT * FROM "TaskItems";

-- Show execution plan for a query
EXPLAIN ANALYZE SELECT * FROM "TaskItems" WHERE "IsCompleted" = true;

-- Count all tasks
SELECT COUNT(*) FROM "TaskItems";

-- Update a task
UPDATE "TaskItems" SET "IsCompleted" = true WHERE "Id" = 1;

-- Delete all completed tasks
DELETE FROM "TaskItems" WHERE "IsCompleted" = true;
```

## Architecture: Storage Abstraction

The **Repository Pattern** proves its value here:

- **Interface**: `IRepoTaskItem.cs` (unchanged from SQLite version)
- **Implementation**: `RepoTaskItem.cs` (uses EF Core, swappable between databases)
- **Service**: `TasksController.cs` (unchanged)
- **Routes**: All endpoints (unchanged)

**Only the database connection string and provider changed.** No service logic, no controller logic, no endpoint URLs changed. This separation of concerns is why you can swap SQLite → PostgreSQL → MySQL without rewriting the API layer. That's the architecture proving itself.

## Endpoints

| Method | Path          | Description                      | Success | Errors   |
| ------ | ------------- | -------------------------------- | ------- | -------- |
| GET    | `/`           | API name, version, main endpoint | 200     |          |
| GET    | `/health`     | Liveness check                   | 200     |          |
| GET    | `/tasks`      | List all tasks                   | 200     |          |
| GET    | `/tasks/{id}` | Get one task by id               | 200     | 404      |
| POST   | `/tasks`      | Create a task                    | 201     | 400      |
| PUT    | `/tasks/{id}` | Update a task                    | 200     | 400, 404 |
| DELETE | `/tasks/{id}` | Delete a task                    | 204     | 404      |

### Status codes

`200` read · `201` created · `204` deleted · `400` invalid body ·
`404` unknown id. Every error returns a JSON body such as
`{ "error": "Task 99 not found" }`.

## Example output

Create a task:

```console
$ curl -i -X POST http://localhost:5241/tasks \
    -H "Content-Type: application/json" \
    -d '{"title":"Buy milk"}'

HTTP/1.1 201 Created
Content-Type: application/json; charset=utf-8
Date: Tue, 29 Sep 2026 16:52:09 GMT
Server: Kestrel
Location: http://localhost:5241/tasks/1
Transfer-Encoding: chunked

{"id":1,"title":"Buy milk","isCompleted":false}
```

Ask for a task that does not exist:

```console
$ curl -i http://localhost:5241/tasks/99

HTTP/1.1 404 Not Found
Content-Type: application/json; charset=utf-8
Date: Tue, 29 Sep 2026 16:52:10 GMT
Server: Kestrel
Transfer-Encoding: chunked

{"error":"Task 99 not found"}
```

## Swagger UI

![Swagger UI showing all endpoints](docs/swagger-ui.png)

Every endpoint above is listed at `/docs` with a working **Try it out**
button, so the full CRUD cycle can be driven from the browser.

## Testing from the editor

`tasks/tasks.http` contains a ready-made request for every endpoint. In
Visual Studio, open the file and click the *send request* link above any
request; in Rider or VS Code, use a REST client extension.

## How it is put together

```
tasks/                       # Main application directory
  Controllers/
    HomeController.cs        # GET / and GET /health
    TasksController.cs       # CRUD endpoints
  Entitys/
    AppDbContext.cs          # Entity Framework Core DbContext
    Models/
      TaskItem.cs            # Task entity model
  Reposetry/
    IRepos/
      IRepoTaskItem.cs       # Storage interface (database-agnostic)
    Repos/
      RepoTaskItem.cs        # PostgreSQL implementation (EF Core)
  Migrations/
    *_InitialCreate.cs       # Database schema migration
  Program.cs                 # Configuration, DI, database initialization
  appsettings.json           # Default settings
  appsettings.Development.json # Development overrides
  .env                       # Environment variables (gitignored)
  .env.example               # Template for .env (committed)
  tasks.csproj               # Project file with NuGet dependencies
  tasks.http                 # REST client test requests

Dockerfile                   # Container image definition
docker-compose.yml           # Multi-container orchestration
init.sql                     # PostgreSQL initialization script

docs/
  swagger-ui.png             # API documentation screenshot
```

## Storage

Tasks are stored in PostgreSQL running in Docker. On startup:
1. Docker Compose starts PostgreSQL with a persistent volume
2. EF Core migrations run automatically
3. Schema is created if missing
4. Sample data is inserted (only once)
5. API connects and serves requests

Data persists in the `postgres_data` Docker volume, surviving container restarts.

### Known deviation from the brief: `isCompleted`, not `done`

The brief's examples use `done` for the completion flag. This API uses
`isCompleted`, which is a deliberate choice and not an oversight.

A client built from the brief will send:

```json
{ "title": "Buy milk", "done": true }
```

and get **400 Bad Request**, with the framework's validation body:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "createTaskDto": ["The createTaskDto field is required."],
    "$.done": ["The JSON property 'done' could not be mapped to any .NET member contained in type 'tasks.Controllers.CreateTaskDto'."]
  }
}
```

The correct call against this API is:

```json
{ "title": "Buy milk", "isCompleted": true }
```

A 400 is the intended response, and JSON is configured to
[reject unmapped members](https://learn.microsoft.com/dotnet/standard/serialization/system-text-json/unmapped-member-handling)
so the mistake is reported rather than swallowed. An earlier version accepted
`done`, returned **201 or 200, and silently discarded it** — the worst outcome
available, because the client would believe the task had been updated when
nothing had changed. That defect was caught by the Stage 7 review, not by
reading the code.

One rough edge worth naming: the `PUT` guard returns this API's own error
shape, `{"error": "Supply at least one of 'title' or 'isCompleted'"}`, while
an unknown *field* returns the longer framework body above. The two error
formats are not unified, and the framework version names internal DTO types.
The AI's version funnels every error through one `ErrorResponse` record, which
is the tidier approach; unifying this is left undone rather than hidden.

See [`ai-version/DIFF-NOTES.md`](ai-version/DIFF-NOTES.md).

## AI vs me (Stage 7)

Building the same API again with an AI, then reviewing its output against
this hand-built version.

- Full prompt: [`ai-version/PROMPT.md`](ai-version/PROMPT.md)
- Full review with measured results: [`ai-version/DIFF-NOTES.md`](ai-version/DIFF-NOTES.md)
- The AI's code: [`ai-version/generated/`](ai-version/generated/) (quarantined, not part of this submission)

> **Disclosure:** the prompt was authored by an AI assistant and the review
> below was performed by that same assistant, not by the author of the
> hand-built `tasks/` version. Every status code was measured against a
> running instance of each API. The judgement step — which is the part Stage 7
> exists to teach — was not performed by the same person who wrote `tasks/`.

Both APIs were started fresh and given the same 20 requests, then a second
round of edge cases the brief never mentions.

### Four defects this found in the hand-built version — all fixed

1. **`PUT` with `{}` returned 200.** The brief requires 400. The guard
   `updateTaskDto == null` only fires when the body is *absent* — `{}`
   deserialises to a non-null object with all fields null, so every guard was
   skipped and the task came back unchanged with a success code. Invisible on
   inspection; it took an independent implementation of the same spec to expose
   it.
2. **Swagger documented `POST /tasks` as 200.** It returns 201. The published
   docs contradicted the API.
3. **Swagger listed no error responses at all.** `GET`, `PUT` and `DELETE` each
   showed only `200` — no 400, no 404, no 204 on delete. Both Swagger defects
   came from missing `[ProducesResponseType]` attributes, now on every action.
4. **Swagger published no descriptions for any of the 7 endpoints.** The
   requirement is that every endpoint is documented, and the source did carry
   XML doc comments — but they never reached `swagger.json`, because
   `GenerateDocumentationFile` was off and Swashbuckle was not pointed at the
   XML. Both halves are now wired, and all 7 operations plus all 3 schemas
   publish a description.

Two of the four had nothing to do with the AI's correctness — they surfaced
only from the extended battery, and #4 surfaced only from auditing
`swagger.json` field by field rather than reading the Swagger page.

### Checkpoint results after the fixes

| Request | Hand-built | AI | Same? |
| ------- | ---------- | -- | ----- |
| `PUT /tasks/1` with `{}` | 400 | 400 | yes — **after the fix** |
| `PUT /tasks/1` with `{"done":true}` | **400** | **200, applied** | **no — deliberate deviation** |
| `PUT /tasks/1` with `{"isCompleted":true}` | 200, applied | 400 | no — schema differs |
| `GET /tasks/abc` | 400 | 404 | no — neither was specified |
| `POST` title `"  pad  "` | trimmed to `"pad"` | kept as `"  pad  "` | no — hand-built better |
| `POST` with no `Content-Type` | **415** | **415** | yes — **both wrong** |
| seed data | one task completed | all three false | Hand-built, marginally |
| Swagger response codes | all four operations complete | all four complete | yes — **after the fix** |

**What the AI did better.** It exposed the `PUT` no-op bug. It used the brief's
field name, so a spec-conforming client works against it; the hand-built
version silently discarded `{"done":true}`, and now at least returns 400. It
documented every response code correctly from the start — its `swagger.json`
already listed `201 400` on create and `200 400 404` on update, where the
hand-built version listed `200` and nothing else, which is the difference
between a documented API and a decorated one. It funnelled every error through
one `ErrorResponse` record instead of four anonymous objects, returned snapshots
rather than handing out the live list, and produced tidier C# — `sealed`,
`init`-only, collection expressions.

**What it got wrong or ignored.** It does not trim titles, so whitespace a user
typed is stored verbatim. It added an `[HttpGet("{id:int}")]` route constraint
making `/tasks/abc` return 404 rather than 400 — defensible, but specified by
nobody. It invented its own seed tasks, split `/` and `/health` into a separate
controller, and shipped no `.http` request collection, so its checkpoints cannot
be replayed from the editor. It also silently drops unrecognised fields, which
the hand-built version did too until this review fixed both.

**A correction to an earlier claim here.** The AI does *not* guarantee 400
rather than 422. That holds for a malformed body, and it does return 400 for
`{}`, but a request with no `Content-Type` never reaches its model-state
factory and comes back **415** from the framework. Both implementations trip
over this, so it is not a point in the AI's favour. Relatedly, the two id
schemes were initially described here as a bug in the hand-built version. They
are not: `Max(id) + 1` only falls back to 1 when the list is genuinely empty,
at which point no such task exists. A design divergence, not a defect.

**What the prompt forgot to specify.** Non-numeric ids; the exact JSON field
names; a consistent error body shape; whether to trim titles; which response
codes the OpenAPI document must list; whether seed data should exercise the
completion flag; whether a request collection was wanted. Each was decided
silently and each changed the output. The no-op `PUT` defect is the exception —
the prompt *did* say 400 and the implementation ignored it, so that is a
reading failure rather than a specification failure.

**The rematch.** Not run. The identified improvement is an explicit table in
the prompt covering non-numeric ids, the no-op `PUT`, exact field names, and
the required response codes — the only four areas where the implementations
diverged, and all four fit in a prompt line. Everything else matched on the
first attempt, which is the more useful result: a specification naming
endpoints, status codes and validation rules gets most of the way there, and
the residue is exactly where judgement is required.



