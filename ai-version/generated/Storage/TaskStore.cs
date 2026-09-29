using TaskApi.Models;

namespace TaskApi.Storage;

/// <summary>
/// In-memory task storage. Holds a single <see cref="List{TaskEntity}"/> for the
/// lifetime of the process; there is no database and no persistence to disk.
/// </summary>
public sealed class TaskStore
{
    private readonly object _gate = new();
    private readonly List<TaskEntity> _tasks =
    [
        new() { Id = 1, Title = "Buy milk", Done = false },
        new() { Id = 2, Title = "Walk the dog", Done = false },
        new() { Id = 3, Title = "Write the report", Done = false },
    ];

    private int _nextId = 4;

    /// <summary>Returns a snapshot of all stored tasks.</summary>
    public IReadOnlyList<TaskEntity> All()
    {
        lock (_gate)
        {
            return [.. _tasks];
        }
    }

    /// <summary>Returns the task with the given id, or <c>null</c> when it does not exist.</summary>
    public TaskEntity? Find(int id)
    {
        lock (_gate)
        {
            return _tasks.FirstOrDefault(task => task.Id == id);
        }
    }

    /// <summary>Adds a task with the next free server-assigned id and <c>done = false</c>.</summary>
    public TaskEntity Add(string title)
    {
        lock (_gate)
        {
            var task = new TaskEntity { Id = _nextId++, Title = title, Done = false };
            _tasks.Add(task);
            return task;
        }
    }

    /// <summary>Applies a partial update. Returns <c>null</c> when the id is unknown.</summary>
    public TaskEntity? Update(int id, string? title, bool? done)
    {
        lock (_gate)
        {
            var index = _tasks.FindIndex(task => task.Id == id);
            if (index < 0)
            {
                return null;
            }

            var updated = new TaskEntity
            {
                Id = id,
                Title = title ?? _tasks[index].Title,
                Done = done ?? _tasks[index].Done,
            };

            _tasks[index] = updated;
            return updated;
        }
    }

    /// <summary>Removes the task with the given id. Returns <c>false</c> when the id is unknown.</summary>
    public bool Delete(int id)
    {
        lock (_gate)
        {
            var removed = _tasks.RemoveAll(task => task.Id == id);
            return removed > 0;
        }
    }
}
