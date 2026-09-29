# Task API

FlyRank Internship — Backend AI Engineering — W2 · A1

A small REST API for managing a to-do list. Create, read, update and delete
tasks; the data lives in memory, so it disappears when the server stops.

Built with **ASP.NET Core on .NET 8**, using MVC controllers and an in-memory
repository registered as a singleton.

## Requirements

- [.NET SDK 8.0 or newer](https://dotnet.microsoft.com/download)

## Run

One command:

```bash
dotnet run --project tasks
```

The server starts on `http://localhost:5241`. Swagger UI is at
**<http://localhost:5241/docs>**.

> If `dotnet run` picks the HTTPS profile instead, either use the
> `https://localhost:7212` base URL, or add `--urls http://localhost:5241` to
> force plain HTTP. The documented commands below assume
> `http://localhost:5241`.

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
Location: http://localhost:5241/tasks/4
Transfer-Encoding: chunked

{"id":4,"title":"Buy milk","isCompleted":false}
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
tasks/
  Controllers/
    HomeController.cs     GET / and GET /health
    TasksController.cs    GET, POST, PUT, DELETE on /tasks
  Models/
    TaskItem.cs           the task entity: Id, Title, IsCompleted
  Reposetry/
    IRepos/
      IRepoTaskItem.cs    storage interface
    Repos/
      RepoTaskItem.cs     in-memory list, seeded with 3 tasks
  Program.cs             wiring, Swagger, DI
  tasks.http             request collection
docs/
  swagger-ui.png         screenshot for this README
```

### Why the repository is a singleton

ASP.NET Core builds a **new controller instance for every request**, so a
plain instance field holding the task list would come back empty on each
call — POST a task, GET `/tasks`, and it would already be gone. Registering
`RepoTaskItem` as a singleton in `Program.cs` makes the list survive between
requests, which is the in-memory stand-in for a database.

## Storage

Tasks are held in a `List<TaskItem>` in memory, seeded with three examples.
Nothing is written to disk, and every task is lost when the server stops.
That is deliberate for this week — the exercise is to notice it, and to see
why a real application needs a database. That is what Week 3 adds.

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

Both APIs were started fresh and given the same 20 requests. They agreed on 16
of them. The four disagreements:

| Request | Hand-built | AI | Which is right |
| ------- | ---------- | -- | -------------- |
| `PUT /tasks/1` with `{}` | 200 | 400 | **AI** — brief says empty body → 400 |
| `PUT /tasks/1` with `{"done":true}` | 200, change discarded | 200, applied | **AI** — field name is `done` in the brief |
| `GET /tasks/abc` | 400 | 404 | Neither — neither was specified |
| seed data | one task `done: true` | all three false | Hand-built, marginally |

**What the AI did better.** It caught a real bug in the hand-built version. The
brief requires `PUT` with an empty body to return 400; the hand-built version
returns 200, because the guard `updateTaskDto == null` only fires when the body
is *absent* — `{}` deserialises to a non-null object with all fields null, so
both guards are skipped and the task is returned unchanged with a success
code. That bug was invisible on inspection and only surfaced because an
independent implementation of the same specification existed to compare
against. It also used the brief's field name, guaranteed 400 rather than 422
via an explicit `InvalidModelStateResponseFactory` instead of inheriting the
framework default, funnelled every error through one `ErrorResponse` record,
and produced noticeably tidier C# — `sealed`, `init`-only properties,
collection expressions, route constraints, snapshot copies.

**What it got wrong or ignored.** It added an `[HttpGet("{id:int}")]` route
constraint that makes `/tasks/abc` return 404 rather than 400 — a defensible
choice, but one nobody specified, and it was the AI's to make or not make. It
invented its own seed tasks, split `/` and `/health` into a separate
controller, and shipped no `.http` request collection, so its checkpoints
cannot be replayed from the editor the way `tasks/tasks.http` allows.

**What the prompt forgot to specify.** Non-numeric ids; the exact JSON field
names; a consistent error body shape; whether seed data should exercise the
`done` flag; whether a request collection was wanted. Each of those was
decided silently and each one changed the output. The empty-body `PUT` defect
is the exception — the prompt *did* say 400, and the implementation ignored
it, so that one is a reading failure rather than a specification failure.

**The rematch.** Not run. The identified improvement is an explicit table in
the prompt covering non-numeric ids, empty-body `PUT` and exact field names —
the only three areas where the implementations diverged. Everything else
matched first try, which is the more useful result: a specification naming
endpoints, status codes and validation rules gets most of the way there, and
the residue is exactly where judgement is required.


