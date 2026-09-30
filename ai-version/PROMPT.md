# My prompt

> **Provenance:** this prompt was written by the assistant, not by hand from
> memory, and the review in `DIFF-NOTES.md` was performed by the assistant
> rather than by the author of the hand-built version. The findings are real
> and empirically verified, but the *judgement* step was not done by the same
> person who built `tasks/`. The lesson of Stage 7 is the judgement step.

## What language and framework?

> ASP.NET Core Web API on **.NET 8**, using **MVC controllers**. Not minimal
> APIs.

## Which endpoints? List every path and method.

| Method | Path | Behaviour |
|---|---|---|
| GET | `/` | 200, `{"name":"Task API","version":"1.0","endpoints":["/tasks"]}` |
| GET | `/health` | 200, `{"status":"ok"}` |
| GET | `/tasks` | 200, array of all tasks |
| GET | `/tasks/{id}` | 200 + task, or 404 + `{"error":"Task 99 not found"}` |
| POST | `/tasks` | `{"title":"..."}` → 201 + created task; missing/empty/whitespace title → 400 |
| PUT | `/tasks/{id}` | optional `title` and/or `done` → 200 + updated; unknown id → 404; invalid body → 400 |
| DELETE | `/tasks/{id}` | 204 empty body; unknown id → 404 |

## Which status codes, and for which cases?

> 200 read · 201 created · 204 deleted with an **empty** body · 400 invalid
> body · 404 unknown id. **Every** error must carry a JSON body — never a
> bare status code. A malformed JSON body must produce **400, not 422**.

## What are the validation rules?

> `title` is required on create and must be a non-empty, non-whitespace
> string. The server assigns `id` and `done` — a client may not choose them.

## How is data stored?

> In-memory only. A `List` seeded with **3 example tasks**. No database, no
> ORM, no EF Core, no file writes. Data must survive between HTTP requests.

## How is the API documented and tested?

> Swagger UI served at **`/docs`** — not the default `/swagger`. Every
> endpoint listed with a description.

## Anything else worth specifying?

> Port 5299. Include the `.csproj` so the project is runnable as delivered.
> Only `Swashbuckle.AspNetCore` permitted as a package. Target `net8.0`. **Do
> not** enable HTTPS redirection — plain HTTP must return 200, not a 307.
> Controllers plus a plain in-memory list; do **not** build a repository
> pattern or a service layer.

## Gaps I found while writing this

*What the AI then did with each of these gaps is in `DIFF-NOTES.md` — that
list is the actual output of this exercise.*

- I never specified what should happen for a **non-numeric** id such as
  `/tasks/abc`. 400 or 404? I had not thought about it at all.
- I never specified what a **PUT with a completely empty body** should do.
  My hand-built version returned 200 for it; the brief says empty body → 400.
  The Stage 7 review found and fixed that, but the prompt was right and the
  implementation was not — a reading failure, not a specification gap.
- I never specified the **JSON field name** for the completion flag. I wrote
  `done` in the brief, then used `isCompleted` in my own code, and never
  reconciled the two. I have now decided to keep `isCompleted` and document the
  deviation, so the gap is closed by choice rather than by accident — but only
  because the review forced the question. Left to myself I would have shipped
  the mismatch silently.
- I never specified whether error bodies should share a consistent shape.
  I used anonymous objects; the AI used a single `ErrorResponse` record.
- I never specified which **status codes the Swagger document must list**. I
  omitted them entirely, so my published docs advertised `200` for a `POST`
  that returns 201, and showed no 400 or 404 anywhere. The AI listed them all
  without being asked. This was the worst of the omissions, because the docs
  then actively misdescribe the API.
