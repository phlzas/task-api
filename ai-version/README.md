# Stage 7 — AI rematch

This folder is a **quarantine zone**. Nothing in here is part of your
Stages 0–6 submission. The hand-built code in `tasks/` stays untouched and
stays the thing you submit.

## The rule

Build the same Task API again, this time by prompting an AI. Any assistant
works — Claude, ChatGPT, Gemini, Copilot. Then review its work the way a
senior engineer reviews a junior's.

You are not testing the AI. You are testing **your prompt**. The only way to
know whether a specification is good is to have built the thing yourself
first, so you can see what got lost.

## Steps

### 1. Write the prompt from memory

Open `PROMPT.md` and fill it in **before** you touch the AI. Do not look at
`tasks/` while you do this. The point is to find out how much of the
specification you actually retained, and the gaps you find are the whole
exercise.

### 2. Generate into a subfolder

Put the AI's output in `generated/`, not loose in this folder:

```
ai-version/generated/
```

Ask for a complete, runnable project — including a `.csproj`. If you forget
to ask for that, its code will not build, and that is a finding worth
writing down. Do not fix it for the AI; that is what a real reviewer would
not do either.

Keep your own `PROMPT.md` outside `generated/`, so the diff never mixes your
words with its code.

### 3. Run it

```bash
dotnet run --project ai-version/generated
```

It must start on **its own port**. Do not let it fight your hand-built API
for port 5241 — pick something else and pass it:

```bash
dotnet run --project ai-version/generated --urls http://localhost:5299
```

Then fire your Stage 4 checkpoint requests at it. Which pass? Which fail?

### 4. Diff it

```bash
git diff --no-index tasks/Controllers/TasksController.cs ai-version/generated/Controllers/TasksController.cs
git diff --no-index tasks/Program.cs ai-version/generated/Program.cs
```

Paths will not match exactly — that is fine, and itself a finding.

### 5. Write it up

Fill in `DIFF-NOTES.md`. Then copy your answers into the **AI vs me**
section of the main `README.md`, along with your full prompt.

## The one question that matters

Each time an AI silently decides something you never specified, write it
down. That list — the things it assumed — is the actual skill. You will
write specifications for the rest of your career, and every assumption the
AI makes is a line you failed to write.

## What not to do

- Do not merge the AI's code into `tasks/`. It stays quarantined.
- Do not polish the AI's code before reviewing it. Ugly output is a finding.
- Do not skip this because it is optional. It is the part that teaches the
  most, and it is the part that separates people who have shipped from
  people who have copied.
