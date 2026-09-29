using Microsoft.AspNetCore.Mvc;
using tasks.Models;
using tasks.Reposetry.IRepos;

namespace tasks.Controllers;

/// <summary>
/// Tasks CRUD API - Stages 2, 3 and 4 live here — the whole CRUD API.
/// </summary>
[ApiController]
[Route("/tasks")]
public class TasksController : ControllerBase
{
    private readonly IRepoTaskItem _repoTaskItem;

    public TasksController(IRepoTaskItem repoTaskItem)
    {
        _repoTaskItem = repoTaskItem;
    }

    // =============================================================
    // STAGE 2 — Read
    // =============================================================

    /// <summary>
    /// Get all tasks
    /// </summary>
    /// <returns>List of all tasks</returns>
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_repoTaskItem.Items);
    }

    /// <summary>
    /// Get a single task by ID
    /// </summary>
    /// <param name="id">The task ID</param>
    /// <returns>The task with the specified ID</returns>
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var task = _repoTaskItem.GetItem(id);
        if (task == null)
            return NotFound(new { error = $"Task {id} not found" });

        return Ok(task);
    }

    // =============================================================
    // STAGE 3 — Create
    // =============================================================

    /// <summary>
    /// Create a new task
    /// </summary>
    /// <param name="createTaskDto">The task title</param>
    /// <returns>The newly created task with ID</returns>
    [HttpPost]
    public IActionResult Create([FromBody] CreateTaskDto createTaskDto)
    {
        // Validation: title is required and cannot be empty
        if (string.IsNullOrWhiteSpace(createTaskDto?.Title))
            return BadRequest(new { error = "Title is required" });

        // Generate next free ID
        var nextId = _repoTaskItem.Items.Any()
            ? _repoTaskItem.Items.Max(x => x.Id) + 1
            : 1;

        var newTask = new TaskItem
        {
            Id = nextId,
            Title = createTaskDto.Title.Trim(),
            IsCompleted = false
        };

        _repoTaskItem.AddItem(newTask);

        return CreatedAtAction(nameof(GetById), new { id = newTask.Id }, newTask);
    }

    // =============================================================
    // STAGE 4 — Update & Delete
    // =============================================================

    /// <summary>
    /// Update an existing task
    /// </summary>
    /// <param name="id">The task ID to update</param>
    /// <param name="updateTaskDto">The updated task data</param>
    /// <returns>The updated task</returns>
    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] UpdateTaskDto updateTaskDto)
    {
        if (updateTaskDto == null)
            return BadRequest(new { error = "Request body is required" });

        var task = _repoTaskItem.GetItem(id);
        if (task == null)
            return NotFound(new { error = $"Task {id} not found" });

        // Validate title if provided
        if (updateTaskDto.Title != null && string.IsNullOrWhiteSpace(updateTaskDto.Title))
            return BadRequest(new { error = "Title cannot be empty" });

        // Update title if provided
        if (updateTaskDto.Title != null)
            task.Title = updateTaskDto.Title.Trim();

        // Update IsCompleted if provided
        if (updateTaskDto.IsCompleted.HasValue)
            task.IsCompleted = updateTaskDto.IsCompleted.Value;

        return Ok(task);
    }

    /// <summary>
    /// Delete a task by ID
    /// </summary>
    /// <param name="id">The task ID to delete</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var removed = _repoTaskItem.RemoveItem(id);
        if (!removed)
            return NotFound(new { error = $"Task {id} not found" });

        return NoContent();
    }
}

/// <summary>
/// DTO for creating a new task
/// </summary>
public class CreateTaskDto
{
    public string? Title { get; set; }
}

/// <summary>
/// DTO for updating an existing task
/// </summary>
public class UpdateTaskDto
{
    public string? Title { get; set; }
    public bool? IsCompleted { get; set; }
}
