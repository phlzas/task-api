namespace TaskApi.Models;

/// <summary>Request body for creating a task. The id is always assigned by the server.</summary>
public sealed class CreateTaskRequest
{
    /// <summary>Title of the task. Required and must not be empty or whitespace.</summary>
    public string? Title { get; init; }
}

/// <summary>Request body for updating a task. Every field is optional.</summary>
public sealed class UpdateTaskRequest
{
    /// <summary>New title. When supplied it must not be empty or whitespace.</summary>
    public string? Title { get; init; }

    /// <summary>New completion state.</summary>
    public bool? Done { get; init; }
}

/// <summary>Uniform JSON error body returned for every failure status code.</summary>
/// <param name="Error">Human readable explanation of what went wrong.</param>
public sealed record ErrorResponse(string Error);
