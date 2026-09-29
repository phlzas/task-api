# AI vs me — review notes

> **Provenance:** the prompt was authored by the assistant and this review was
> performed by the assistant, not by the author of the hand-built `tasks/`
> version. Every status code below was measured against a running instance of
> each API on a fresh server, not read off the source. The judgement step —
> which is the actual skill Stage 7 teaches — was not done by the same person
> who built the hand version.

The hand-built version lives in `tasks/` and runs on port **5241**. The AI's
version lives in `generated/` and runs on port **5299**. Both were started
fresh before each round of requests, because an earlier round of testing was
contaminated by a stale process still holding a port — worth knowing, because
it produced two confidently wrong readings before it was caught.

## Did it start on the first try?

Yes. `dotnet build` returned 0 errors and 0 warnings, and the server came up
and answered requests without incident. It did not need a retry.

One process note: the build-and-run step had to be stopped and finished
separately, because `dotnet run` does not return. That is a property of the
command, not a defect in the generated code.

## Checkpoint results

Measured, not assumed. The hand version returned 200 to the brief's field
name `done` while silently discarding it — that cell is the interesting one.

| Request | Mine | The AI's | Same? |
| ------- | ---- | -------- | ----- |
| `GET /` | 200 | 200 | yes |
| `GET /health` | 200 | 200 | yes |
| `GET /tasks` | 200 | 200 | yes |
| `GET /tasks/1` | 200 | 200 | yes |
| `GET /tasks/99` | 404 | 404 | yes |
| `GET /tasks/abc` | **400** | **404** | **no** |
| `GET /tasks/-5` | 404 | 404 | yes |
| `POST /tasks` valid | 201 | 201 | yes |
| `POST /tasks` empty body | 400 | 400 | yes |
| `POST /tasks` blank title | 400 | 400 | yes |
| `POST /tasks` malformed JSON | 400 | 400 | yes |
| `PUT /tasks/1` blank title | 400 | 400 | yes |
| `PUT /tasks/1` empty body `{}` | **200** | **400** | **no — mine is wrong** |
| `PUT /tasks/1` `{"done":true}` | **200, ignored** | **200, applied** | **no — mine is wrong** |
| `PUT /tasks/1` `{"isCompleted":true}` | 200, applied | 400 | no — schema differs |
| `PUT /tasks/99` | 404 | 404 | yes |
| `DELETE /tasks/1` | 204 | 204 | yes |
| `DELETE /tasks/99` | 404 | 404 | yes |
| `GET /docs` | 200 | 200 | yes |

## 1. What did the AI do better?

**It caught a real bug in my version.** The brief says an empty or invalid
body on `PUT` must return 400. My version returns **200**. The cause is a
subtle one: `updateTaskDto == null` only fires when the body is *absent*, but
`{}` deserialises into a non-null object whose fields are all null, so both
of my guards are skipped and the method returns the task unchanged with
200. The AI guarded explicitly — *"Supply at least one of 'title' or
'done'"* — and returns 400. Reading my own code, that bug was invisible. It
took a working implementation of the same spec to expose it.

**It followed the field name in the brief.** I wrote `done` in the
specification and then named the property `isCompleted` in my own code. A
client built to the brief sends `{"done":true}`, my API binds nothing and
returns 200 having changed nothing — silent data loss behind a success code.
The AI used `done` exactly as specified.

**It guaranteed the 400-not-422 rule rather than inheriting it.** It
registered an `InvalidModelStateResponseFactory` so a malformed body is
converted to a 400 with a JSON body. My version returns 400 too, but only
because that is the framework default — I never made it a decision, so I
could not have told you why it works.

**A single error shape.** It uses `record ErrorResponse(string Error)` for
every failure, so all errors serialise to `{"error":"..."}`. Mine uses
anonymous objects at four separate call sites. Same output today, but nothing
guarantees the fourth one will match if someone edits it.

**Cleaner C# throughout.** `sealed` classes, `init`-only properties,
collection expressions (`[.. _tasks]`), file-scoped namespaces,
`{id:int}` route constraints, and it returns snapshots from `All()` rather
than exposing the live list. It also set `GenerateDocumentationFile` with
`NoWarn 1591`, which fills Swagger with real descriptions.

**It did not over-build.** Told to keep it simple, it produced a controller,
a store and three small records — no repository interface, no service layer.
It obeyed a negative instruction, which is rarer than it sounds.

## 2. What did it get wrong, or quietly ignore from the prompt?

**It added a route constraint I never asked for.** `[HttpGet("{id:int}")]`
means `/tasks/abc` returns **404** where mine returns **400**. Neither is
wrong, but I never specified it, and the two answers are defensible — a
non-numeric id *is* a malformed request (400) and *is* an absent resource
(404). The AI chose silently. This is the cleanest example on this page of
something an AI decides for you unless you write it down.

**It invented seed data.** My three tasks are `Finish the Week 2 README` and
`Review the HTTP status codes` — self-referential, and frankly a bit smug. It
chose `Walk the dog` and `Write the report`. Mine also seeds one task with
`done: true`, which demonstrates the flag better; the AI seeds all three
false, so the Swagger screenshot does not show the boolean doing anything.

**It split `/` and `/health` into a separate `ApiInfoController` and gave them
named response types.** Fine, arguably tidier, but it is a structural choice
I did not make and would not have made.

**It has no request collection.** `tasks/tasks.http` gives a one-click request
per endpoint in the editor. The generated project has nothing equivalent, so
running its checkpoints means remembering URLs.

**Its id counter is a plain increment.** `_nextId = 4`, then `_nextId++`. Mine
computes `Max(x => x.Id) + 1` on every create. The AI's is cheaper and cannot
collide here, because clients cannot choose ids — but it is a hidden
assumption rather than a derived fact.

## 3. What did my prompt forget to specify?

Every one of these is a line I did not write, and every one changed the
output:

1. **Non-numeric ids.** Unspecified. The AI chose 404 via a route
   constraint. I would have chosen 400. Both shipped without me deciding.
2. **Empty-body PUT.** I knew the brief said 400 and still wrote code that
   returns 200. The prompt was not the failure here — the implementation was.
   Worth being precise about: this one is on me, not on the AI.
3. **The `done` vs `isCompleted` field name.** I specified `done` and
   implemented `isCompleted`. Nobody reconciled them, so the API and its own
   specification disagree.
4. **A consistent error body shape.** Unspecified, so the AI chose a record
   and I chose four anonymous objects.
5. **Whether seed data should exercise `done: true`.** Unspecified.
6. **Whether a `.http` request collection was wanted.** Unspecified, so I have
   one and it does not.
7. **The project name and namespace.** It chose `TaskApi`; mine is `tasks`.
   Harmless, but it means `git diff --no-index` paths do not line up.
8. **The `/tasks` route shape** — `[Route("tasks")]` versus my
   `[Route("/tasks")]`. Identical behaviour, different text, and a diff tool
   reports it as a change.

## The rematch

The prompt was not regenerated for a second attempt, so there is no second
round to report. The improvement identified from this round is a concrete
one: add an explicit table to the prompt covering non-numeric ids, empty-body
PUT, and the exact JSON field names, since those three are the only places
the two implementations disagree. Everything else already matched on the first
attempt — which is the more useful lesson, because it means a specification
that names endpoints, status codes and validation rules gets most of the way
there, and the residue is exactly where the judgement lives.
