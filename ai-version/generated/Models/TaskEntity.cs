namespace TaskApi.Models;

/// <summary>A single to-do item held in memory.</summary>
public sealed class TaskEntity
{
    /// <summary>Server-assigned identifier, starting at 1.</summary>
    public int Id { get; init; }

    /// <summary>Short description of the task.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>Whether the task has been completed.</summary>
    public bool Done { get; init; }
}
