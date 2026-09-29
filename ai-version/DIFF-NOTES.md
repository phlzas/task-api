# AI vs me — review notes

> **Provenance:** the prompt was authored by an AI assistant and this review was
> performed by that same assistant, not by the author of the hand-built `tasks/`
> version. Every status code below was measured against a running instance of
> each API, not read off the source. The judgement step — which is the actual
> skill Stage 7 teaches — was not done by the same person who built the hand
> version.

The hand-built version lives in `tasks/` and runs on port **5241**. The AI's
version lives in `generated/` and runs on port **5299**.

Two process notes, because both produced confidently wrong readings before
they were caught:

- An early round of testing ran against a **stale `dotnet run` orphan** still
  holding port 5299, already mutated by earlier requests. That made a working
  `PUT` look like a 404. Every number below comes from an instance confirmed
  fresh by checking its seed data first.
- A verification run after a failed build **silently tested the previous
  binary**, and one test fixture had been written as an empty file instead of
  `{}`, so an apparent `PUT {}` pass was really a JSON parse error. The build
  is now gated on a clean exit before any request is sent.

## Did it start on the first try?

Yes. `dotnet build` returned 0 errors and 0 warnings, and the server came up
and answered requests without incident.

One process note: `dotnet run` does not return, so build-and-run has to be
stopped and finished separately. That is a property of the command, not a
defect in the generated code.

## What the review found in the hand-built version

Three real defects, all now fixed. Two of the three had nothing to do with the
AI's correctness — the extended battery found them by testing cases the first
pass never exercised.

### 1. `PUT` with a no-op body returned 200 — fixed

The brief says an invalid body on `PUT` must return 400. Sending `{}` — or
`{"title":null,"isCompleted":null}` — returned **200** with the task
unchanged. Cause: `updateTaskDto == null` only fires when the body is
*absent*, but `{}` deserialises into a **non-null** object whose fields are all
null, so every guard was skipped.

Fixed with an explicit check. `404` still takes precedence over `400` for an
unknown id, which is the conventional order and the one the original code
already implied.

### 2. Swagger documented `POST /tasks` as returning 200 — fixed

It returns **201**. The generated `swagger.json` said `200`, so the published
documentation contradicted the API.

### 3. Swagger documented no error responses at all — fixed

`GET`, `PUT` and `DELETE` each listed only `200`. No `400`, no `404`, no `204`
on delete. Anyone reading the docs could not learn that a delete returns 204 or
that a bad body returns 400.

Both Swagger defects came from the same cause: no `[ProducesResponseType]`
attributes, which are now on every action.

## Checkpoint results

Measured against a clean build. The three defects above are fixed in this
table.

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
| `PUT /tasks/1` empty body `{}` | 400 | 400 | yes — **after the fix** |
| `PUT /tasks/1` explicit nulls | 400 | 400 | yes — **after the fix** |
| `PUT /tasks/1` `{"done":true}` | **400** | **200, applied** | **no** |
| `PUT /tasks/1` `{"isCompleted":true}` | 200, applied | 400 | no — schema differs |
| `PUT /tasks/99` | 404 | 404 | yes |
| `DELETE /tasks/1` | 204 | 204 | yes |
| `DELETE /tasks/99` | 404 | 404 | yes |
| `GET /docs` | 200 | 200 | yes |

## The extended battery

The first pass only covered the endpoints the brief names. A second pass went
looking for the cases a real client hits and a spec never mentions. Two of the
three defects came from here.

| Case | Mine | The AI's | Same? |
| ---- | ---- | -------- | ----- |
| `POST` title is a number | 400 | 400 | yes |
| `POST` array body | 400 | 400 | yes |
| `POST` string body | 400 | 400 | yes |
| `POST` with no `Content-Type` | **415** | **415** | yes — **both wrong** |
| `POST {"title":"Buy milk","done":true}` | **400** | 201, `done` dropped | **no — fixed** |
| `POST {"id":99,"done":true}` | **400** | 201, both dropped | no — fixed |
| `POST` title `"  pad  "` | trimmed to `"pad"` | **kept as `"  pad  "`** | **no — mine better** |
| `GET /tasks/` trailing slash | 200 | 200 | yes |
| `GET /TASKS` uppercase | 200 | 200 | yes — routing is case-insensitive |
| `GET /tasks/2147483647` | 404 | 404 | yes |
| first id after deleting every task | 1 | 4 | design divergence, not a bug |
| Swagger `POST /tasks` | 201 400 | 201 400 | yes — **after the fix** |
| Swagger `GET /tasks/{id}` | 200 404 | 200 404 | yes — **after the fix** |
| Swagger `PUT /tasks/{id}` | 200 400 404 | 200 400 404 | yes — **after the fix** |
| Swagger `DELETE /tasks/{id}` | 204 404 | 204 404 | yes — **after the fix** |

## 1. What did the AI do better?

**It exposed the `PUT` no-op bug.** Defect 1 above. Reading the code, that bug
was invisible; it took an independent implementation of the same specification
to make it obvious.

**It followed the field name in the brief.** I wrote `done` in the
specification and named the property `isCompleted` in my own code. Before the
fixes, a spec-conforming client got **200 on `PUT` and 201 on `POST`, both
having silently discarded the field** — data loss behind a success code. It
now gets 400 on both. The AI's version uses `done` and applies it.

**It documented every response code correctly from the start.** Its
`swagger.json` already listed `201 400` on create and `200 400 404` on update,
where mine listed `200` and nothing else. That is the difference between a
documented API and a decorated one, and it took three attributes per action to
catch up.

**A single error shape.** `record ErrorResponse(string Error)` for every
failure, so all errors serialise to `{"error":"..."}`. Mine uses anonymous
objects at four separate call sites — same output today, but nothing stops the
fourth from drifting.

**Cleaner C# throughout.** `sealed` classes, `init`-only properties, collection
expressions, snapshot copies from `All()` rather than handing out the live
list, and `GenerateDocumentationFile` with `NoWarn 1591` so Swagger fills with
real descriptions.

**It did not over-build.** Told to keep it simple, it produced a controller, a
store and three small records — no repository interface, no service layer. It
obeyed a negative instruction, which is rarer than it sounds.

## 2. What did it get wrong, or quietly ignore from the prompt?

**It does not trim titles.** `{"title":"  pad  "}` is stored with the padding
intact. The brief only requires a non-empty, non-whitespace title, so this is
not a violation — but storing whitespace a user typed is a small quality gap
my version does not have.

**It silently drops unrecognised fields.** `POST {"title":"x","done":true}`
returned 201 with the flag discarded, and a client-supplied `id` was accepted
and ignored. The hand-built version had the identical defect; neither
implementation rejected unknown members until this review configured
`UnmappedMemberHandling.Disallow`. So this is not a point in the AI's favour —
it is a gap both shared, and both prompts failed to mention.

**It returns 415 for a missing `Content-Type`, not 400.** So does mine. This
corrects something claimed earlier in this file: the AI does **not** "guarantee"
400 via its `InvalidModelStateResponseFactory`. That holds for a malformed body,
and it does return 400 for `{}` — but a request with no `Content-Type` never
reaches the model-state factory and comes back 415 from the framework. Both
implementations trip over this, so it is not a point in the AI's favour.

**It added a route constraint I never asked for.** `[HttpGet("{id:int}")]`
makes `/tasks/abc` return **404** where mine returns **400**. Both defensible,
neither specified, and its choice to make silently is the cleanest example here
of something an AI decides for you unless you write it down.

**It invented seed data.** Mine are `Finish the Week 2 README` and `Review the
HTTP status codes` — self-referential, and frankly a bit smug. It chose `Walk
the dog` and `Write the report`. Mine also seeds one task with the completion
flag set, so the boolean does something visible; the AI seeds all three false,
so its Swagger screenshot never exercises it.

**It split `/` and `/health` into a separate `ApiInfoController`** with named
response types. Tidier, and a structural choice I did not make.

**It has no request collection.** `tasks/tasks.http` gives a one-click request
per endpoint. The generated project has nothing equivalent, so replaying its
checkpoints means remembering URLs.

## 3. What did my prompt forget to specify?

Every one of these is a line I did not write, and every one changed the output:

1. **Non-numeric ids.** Unspecified. The AI chose 404 via a route constraint;
   I return 400.
2. **The `done` vs `isCompleted` field name.** I specified `done` and
   implemented `isCompleted`, and nobody reconciled them. This stays
   unresolved **by choice** — the hand-built version keeps `isCompleted` and
   documents the deviation, rather than renaming to match. The observable
   consequence is that a spec-conforming client gets 400 rather than a silent
   no-op. That is the remaining divergence between the two APIs, and it is
   documented under "Known deviation" in the main `README.md`.

   Getting to that 400 took a second fix, and it is the more interesting one.
   Rejecting `done` on `PUT` was free — the no-op guard already caught it,
   because a body of `{"done":true}` leaves both fields null. On `POST` the
   same field was accepted and **silently discarded**: `CreateTaskDto` has only
   `Title`, `System.Text.Json` ignores unmapped members by default, and because
   `title` was valid the no-op guard never fired. The result was `201 Created`
   with `isCompleted: false` and a client that believed its flag had been set.
   Fixed by setting `UnmappedMemberHandling.Disallow` in `AddJsonOptions`, so an
   unrecognised field is a 400 on every endpoint. It also now rejects a
   client-supplied `id`, which was previously accepted and ignored.
3. **What to do with a no-op `PUT`.** I said "invalid body → 400" and then
   wrote code returning 200. The prompt was right and the implementation ignored
   it — a reading failure, not a specification failure.
4. **A consistent error body shape.** Unspecified, so the AI chose a record
   and I chose four anonymous objects.
5. **Whether to trim titles.** Unspecified, so the AI keeps padding and I strip
   it. Neither is wrong.
6. **Response codes in the OpenAPI document.** Unspecified, and I omitted them
   entirely — the worst omission, because the docs then actively misdescribe the
   API.
7. **Whether seed data should exercise the completion flag.** Unspecified.
8. **Whether a `.http` request collection was wanted.** Unspecified.
9. **The project name and namespace.** It chose `TaskApi`; mine is `tasks`, so
   `git diff --no-index` paths do not line up.

## A correction I owe the record

I initially treated the two id schemes as a bug in the hand-built version — it
computes `Max(id) + 1`, the AI keeps a monotonic counter, and I assumed the
hand-built one would eventually collide. Testing it properly showed that is
wrong: the `Any()` guard only fires when the list is *genuinely empty*, at which
point no task with that id exists, so there is nothing to collide with. Both
schemes are valid. The AI's never reuses an id, which some prefer; mine keeps
ids small. A design divergence, not a defect.

## The rematch

Not run. The improvements identified from this round are concrete: add an
explicit table to the prompt covering non-numeric ids, the no-op `PUT`, the
exact JSON field names, and the response codes the OpenAPI document must list.
Those four are the only places the implementations disagreed, and all four fit
in a prompt line.

The more useful lesson is the one I got wrong twice. A specification naming
endpoints, status codes and validation rules gets an implementation most of the
way there on the first attempt — and the residue is exactly where judgement is
required. It also will not tell you about itself: three of the four defects I
found in my own code surfaced only when I tested cases the brief never
mentions, and two of my early "findings" were measurement errors, not bugs.
