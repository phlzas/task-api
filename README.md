# Task API

FlyRank Internship — Backend AI Engineering — W2 · A1

A small CRUD API for managing a to-do list, built with ASP.NET Core on .NET 8.

<!-- TODO: one or two sentences on what this is and why it exists. Write this
     first, it is what a reader sees. -->

## Status

Work in progress.

- [ ] Stage 0 — Hello, server
- [ ] Stage 1 — Root and health endpoints
- [ ] Stage 2 — Read endpoints with 404
- [ ] Stage 3 — Create with validation
- [ ] Stage 4 — Full CRUD
- [ ] Stage 5 — Swagger UI
- [ ] Stage 6 — Publish and docs

## Requirements

- [.NET SDK 8.0 or newer](https://dotnet.microsoft.com/download)

## Run

One command:

```bash
dotnet run --project tasks
```

Then open <http://localhost:5241/docs>.

## Endpoints

| Method | Path          | Description | Success | Errors      |
| ------ | ------------- | ----------- | ------- | ----------- |
| GET    | `/`           | TODO        | 200     |             |
| GET    | `/health`     | TODO        | 200     |             |
| GET    | `/tasks`      | TODO        | 200     |             |
| GET    | `/tasks/{id}` | TODO        | 200     | 404         |
| POST   | `/tasks`      | TODO        | 201     | 400         |
| PUT    | `/tasks/{id}` | TODO        | 200     | 400, 404    |
| DELETE | `/tasks/{id}` | TODO        | 204     | 404         |

## Example response

<!-- TODO: run a real `curl -i` and paste the actual output. Do not invent it. -->

```
TODO
```

## Swagger UI

<!-- TODO: paste your screenshot of /docs here. -->

## Storage

Tasks are held in memory. Data is lost when the server stops — that is
deliberate for this week. A database arrives in Week 3.

## AI vs me

<!-- TODO (Stage 7, optional): your full prompt, the AI's code in ai-version/,
     and at least three concrete differences you found between the two. -->
