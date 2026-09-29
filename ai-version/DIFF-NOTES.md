# AI vs me — review notes

Fill this in after running the AI's version. Then copy the answers into the
**AI vs me** section of the main `README.md`.

Do not tidy these up. Specific beats polished. "It returned 422 instead of
400" is worth more than "the validation was slightly off".

---

## Did it start on the first try?

> Yes / No — what happened.

## Checkpoint results

Run the same requests against both and record what each did.

| Request | Mine | The AI's | Same? |
| ------- | ---- | -------- | ----- |
| `GET /` | | | |
| `GET /health` | | | |
| `GET /tasks` | | | |
| `GET /tasks/1` | | | |
| `GET /tasks/99` | | | |
| `POST /tasks` valid | | | |
| `POST /tasks` empty body | | | |
| `POST /tasks` blank title | | | |
| `PUT /tasks/1` | | | |
| `PUT /tasks/99` | | | |
| `DELETE /tasks/1` | | | |
| `DELETE /tasks/99` | | | |
| `GET /docs` | | | |

## 1. What did the AI do better?

> And do you understand its version well enough to explain it? If not, that
> is the honest answer.

## 2. What did it get wrong, or quietly ignore?

> A missing 400? A wrong status code? A database you never asked for? List
> each one concretely.

## 3. What did my prompt forget to specify?

> What did the AI silently decide for you? This is the question that
> actually matters — every item here is a line you did not write.

## The rematch

Improve your prompt with what you learned. Regenerate. One sentence on what
changed:

> e.g. "Adding an explicit status-code table and a line about no database
> fixed X and Y."
