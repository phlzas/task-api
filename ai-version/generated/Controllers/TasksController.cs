using Microsoft.AspNetCore.Mvc;
using TaskApi.Models;
using TaskApi.Storage;

namespace TaskApi.Controllers;

/// <summary>CRUD operations over the in-memory to-do list.</summary>
[ApiController]
[Route("tasks")]
[Produces("application/json")]
public sealed class TasksController : ControllerBase
{
    private readonly TaskStore _store;

    /// <summary>Creates the controller.</summary>
    /// <param name="store">The in-memory task store.</param>
    public TasksController(TaskStore store) => _store = store;

    /// <summary>Lists every task.</summary>
    /// <response code="200">All tasks, seeded with three examples on startup.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskEntity>), StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<TaskEntity>> GetAll() => Ok(_store.All());

    /// <summary>Gets a single task by id.</summary>
    /// <param name="id">Identifier of the task.</param>
    /// <response code="200">The requested task.</response>
    /// <response code="404">No task exists with that id.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public ActionResult<TaskEntity> GetById(int id)
    {
        var task = _store.Find(id);
        return task is null ? NotFoundTask(id) : Ok(task);
    }

    /// <summary>Creates a task. The id and the <c>done</c> flag are assigned by the server.</summary>
    /// <param name="request">Body containing the required title.</param>
    /// <response code="201">The created task.</response>
    /// <response code="400">The title is missing, empty, whitespace, or the body is malformed.</response>
    [HttpPost]
    [ProducesResponseType(typeof(TaskEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public ActionResult<TaskEntity> Create([FromBody] CreateTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new ErrorResponse("Field 'title' is required and must be a non-empty string."));
        }

        var created = _store.Add(request.Title);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates the title and/or the done flag of an existing task.</summary>
    /// <param name="id">Identifier of the task to update.</param>
    /// <param name="request">Body with an optional title and an optional done flag.</param>
    /// <response code="200">The updated task.</response>
    /// <response code="400">A supplied title is empty or whitespace, or the body is malformed.</response>
    /// <response code="404">No task exists with that id.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TaskEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public ActionResult<TaskEntity> Update(int id, [FromBody] UpdateTaskRequest request)
    {
        if (request.Title is not null && string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new ErrorResponse("Field 'title' must be a non-empty string when supplied."));
        }

        if (request.Title is null && request.Done is null)
        {
            return BadRequest(new ErrorResponse("Supply at least one of 'title' or 'done'."));
        }

        var updated = _store.Update(id, request.Title, request.Done);
        return updated is null ? NotFoundTask(id) : Ok(updated);
    }

    /// <summary>Deletes a task.</summary>
    /// <param name="id">Identifier of the task to delete.</param>
    /// <response code="204">Deleted, with an empty body.</response>
    /// <response code="404">No task exists with that id.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id) => _store.Delete(id) ? NoContent() : NotFoundTask(id);

    private NotFoundObjectResult NotFoundTask(int id) =>
        NotFound(new ErrorResponse($"Task {id} not found"));
}
