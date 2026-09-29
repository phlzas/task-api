using Microsoft.AspNetCore.Mvc;
using tasks.Models;

namespace tasks.Controllers;

// Stages 2, 3 and 4 live here — the whole CRUD API.

[ApiController]
[Route("/tasks")]
public class TasksController : ControllerBase
{
    // ################################################################
    // #  READ THIS BEFORE YOU WRITE ANYTHING                        #
    // #                                                              #
    // #  Your tasks list cannot be a plain instance field here.      #
    // #                                                              #
    // #  ASP.NET Core creates a NEW controller instance for EVERY    #
    // #  request. So this:                                         #
    // #                                                              #
    // #      private List<TaskItem> _tasks = new();                  #
    // #                                                              #
    // #  comes back EMPTY on every single call. POST a task, then    #
    // #  GET /tasks, and your new task is gone. It looks like your   #
    // #  code is broken. It is not — this is how MVC works.          #
    // #                                                              #
    // #  You have to make the list survive between requests.        #
    // #  Research how, then pick one:                               #
    // #    - a static field                                         #
    // #    - registering the list as a singleton service            #
    // #    - anything else you find                                  #
    // #                                                              #
    // #  This is the single most common .NET gotcha for this         #
    // #  assignment. Budget 20 minutes for it.                       #
    // ################################################################


    // TODO 1: declare your list of 3 example tasks, here as a field.
    //         Each task: Id (int), Title (string), Done (bool).
    //         e.g. Id 1 "Buy milk" Done false
    //         Solve the persistence problem above FIRST.


    // =============================================================
    // STAGE 2 — Read
    //
    // TODO 2: GET /tasks -> return the whole list, 200
    //
    // TODO 3: GET /tasks/{id} -> return ONE task, 200
    //         Unknown id -> 404 with a JSON error, e.g.
    //         { "error": "Task 99 not found" }
    //         Never return an empty 200 for something that is not
    //         there. Status codes are how machines read your answers.
    //
    // Checkpoint:
    //   curl -i http://localhost:5241/tasks/1   -> 200 + one task
    //   curl -i http://localhost:5241/tasks/99  -> 404 + error JSON
    // Commit: Stage 2: read endpoints with 404
    // =============================================================
    [HttpGet]
    public IActionResult GetAll()
    {
        // TODO 2
        throw new NotImplementedException("Stage 2 — implement me");
    }

    // TODO 3 goes here


    // =============================================================
    // STAGE 3 — Create
    //
    // TODO 4: POST /tasks
    //         - read the new task's title from the request body
    //         - give it the next free id
    //         - Done = false
    //         - add it to the list
    //         - return the created task with 201
    //
    //         C# note: to read a JSON body into a class, declare the
    //         action parameter as that class. e.g.
    //             public IActionResult Create(SomeClass body)
    //         You already know what fields the body needs: just title.
    //
    //         Validation: if title is missing or empty -> 400 with a
    //         JSON error. Good news: ASP.NET Core returns 400 for a
    //         malformed body automatically. The empty-string case is
    //         yours to handle.
    //
    // Checkpoint:
    //   curl -i -X POST http://localhost:5241/tasks \
    //     -H "Content-Type: application/json" -d "{\"title\":\"Buy milk\"}"
    //   -> 201 + the new task; a later GET /tasks shows it
    //   posting {} -> 400
    // Commit: Stage 3: create with validation
    // =============================================================


    // =============================================================
    // STAGE 4 — Update & Delete
    //
    // TODO 5: PUT /tasks/{id}
    //         - replace title and/or done from the body
    //         - return the updated task, 200
    //         - unknown id -> 404, bad body -> 400
    //
    // TODO 6: DELETE /tasks/{id}
    //         - remove the task
    //         - return 204 with an EMPTY body
    //         - unknown id -> 404
    //
    // C# note: returning 204 is one call: NoContent()
    //
    // Checkpoint: create -> update -> mark done -> delete -> GET /tasks
    // Commit: Stage 4: full CRUD
    // =============================================================


    // =============================================================
    // STAGE 5 — Swagger UI
    //
    // The template already has Swagger, but at /swagger.
    // The assignment wants /docs. One line in Program.cs:
    //
    //   app.UseSwaggerUI(options => options.RoutePrefix = "docs");
    //
    // (replace the plain app.UseSwaggerUI() call with that)
    //
    // Also: Properties/launchSettings.json opens the browser at
    // "swagger" — change those launchUrl values to "docs" too, or the
    // browser will 404 when you press F5.
    //
    // Then open /docs, hit "Try it out", and run the full CRUD cycle.
    // Screenshot it for the README.
    //
    // Add a one-line /// <summary> comment to each action above and
    // watch the docs page fill in.
    // Commit: Stage 5: Swagger UI
    // =============================================================
}
