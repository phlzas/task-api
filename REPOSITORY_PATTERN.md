# Repository Pattern & Storage Abstraction

How the architecture enables swapping SQLite ? PostgreSQL without touching the API layer.

## The Problem

If your service layer directly called SQLite APIs:

```csharp
// Bad: Tightly coupled to SQLite
public class TaskService 
{
    public List<TaskItem> GetAll()
    {
        using var connection = new SqliteConnection("...");
        var command = new SqliteCommand("SELECT * FROM tasks", connection);
        // ...
    }
}
```

To switch to PostgreSQL, you'd rewrite this entire class, touching service logic, error handling, caching — everything.

## The Solution: Repository Pattern

Introduce an interface that abstracts data access:

```csharp
// IRepoTaskItem.cs - Database-agnostic interface
public interface IRepoTaskItem
{
    List<TaskItem> Items { get; }
    void AddItem(TaskItem item);
    TaskItem? GetItem(int id);
    bool RemoveItem(int id);
    void UpdateItem(TaskItem item);
}
```

Now services and controllers depend only on this interface:

```csharp
// TasksController.cs - No database knowledge
public class TasksController : ControllerBase
{
    private readonly IRepoTaskItem _repo;

    public TasksController(IRepoTaskItem repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_repo.Items);
    }
}
```

## Swappable Implementations

### SQLite Implementation

```csharp
// RepoTaskItem.cs for SQLite
public class RepoTaskItem : IRepoTaskItem
{
    private readonly AppDbContext _dbContext;

    public RepoTaskItem(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<TaskItem> Items => _dbContext.TaskItems.ToList();

    public void AddItem(TaskItem item)
    {
        _dbContext.TaskItems.Add(item);
        _dbContext.SaveChanges();
    }

    // ... etc
}
```

### PostgreSQL Implementation (Identical!)

```csharp
// Same class name, same interface
// The ONLY difference: DbContext provider changed
public class RepoTaskItem : IRepoTaskItem
{
    private readonly AppDbContext _dbContext;  // Same DbContext!

    public RepoTaskItem(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<TaskItem> Items => _dbContext.TaskItems.ToList();

    public void AddItem(TaskItem item)
    {
        _dbContext.TaskItems.Add(item);
        _dbContext.SaveChanges();
    }

    // Identical implementation!
}
```

## What Changed (SQLite ? PostgreSQL)

### 1. Program.cs - Connection String

**SQLite**:
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=tasks.db"));
```

**PostgreSQL**:
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
```

### 2. Project File - NuGet Package

**SQLite**:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
```

**PostgreSQL**:
```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" />
```

### 3. Database Schema - Migrations

**SQLite**: Auto-created file-based schema

**PostgreSQL**: Migrations run in Docker container

### That's It!

**Controllers**: No changes
**Services**: No changes
**Routes**: No changes
**Interfaces**: No changes
**Repository interface**: No changes

Only the data layer changed. The rest of the application is untouched.

## Dependency Injection: The Glue

In `Program.cs`:

```csharp
// This registration makes everything work
builder.Services.AddScoped<IRepoTaskItem, RepoTaskItem>();

// Controllers get IRepoTaskItem injected
// They never know which implementation they're using
```

When a controller needs the repository:

```csharp
public TasksController(IRepoTaskItem repo)  // Interface, not implementation
{
    _repo = repo;  // Could be SQLite, PostgreSQL, MySQL, Redis...
}
```

## Testing: The Real Payoff

You can inject a mock implementation for unit tests:

```csharp
// Test: Create a mock repository
public class MockRepository : IRepoTaskItem
{
    private readonly List<TaskItem> _items = new();

    public List<TaskItem> Items => _items;

    public void AddItem(TaskItem item) => _items.Add(item);

    // ... simple in-memory implementation for testing
}

// Test: Inject the mock
var mockRepo = new MockRepository();
var controller = new TasksController(mockRepo);

// Test: No database needed, tests are fast
controller.GetAll();  // Uses mock, not database
```

## Evolution Path

This architecture lets you evolve:

```
Week 1: In-memory List<T>
  ?
Week 2: SQLite file-based
  ?
Week 3: PostgreSQL in Docker (this week)
  ?
Week 4: Add Redis caching (new decorator implementation)
  ?
Week 5: Distributed cache (replace Redis with Memcached)
  ?
Production: MySQL, MongoDB, Cosmos DB, etc.
```

**Each migration touches one file**: The `Program.cs` DI registration and that one implementation class.

## Real-World Example: Adding Caching

Wrap the repository in a decorator:

```csharp
public class CachedRepository : IRepoTaskItem
{
    private readonly IRepoTaskItem _inner;
    private readonly IDistributedCache _cache;

    public CachedRepository(IRepoTaskItem inner, IDistributedCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public List<TaskItem> Items
    {
        get
        {
            var cached = _cache.GetString("all_tasks");
            if (cached != null) return JsonConvert.DeserializeObject<List<TaskItem>>(cached);

            var items = _inner.Items;
            _cache.SetString("all_tasks", JsonConvert.SerializeObject(items), 
                new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
            return items;
        }
    }

    // ... delegate other methods to _inner
}
```

Register it:

```csharp
builder.Services.AddScoped<IRepoTaskItem>(sp => 
    new CachedRepository(
        new RepoTaskItem(sp.GetRequiredService<AppDbContext>()),
        sp.GetRequiredService<IDistributedCache>()
    )
);
```

**Controllers**: Still unchanged
**API responses**: Still unchanged
**Routes**: Still unchanged

## Costs vs Benefits

### Costs (Minimal)

- One extra interface definition
- Dependency injection setup
- Slightly more indirection (method calls through interface)

### Benefits (Massive)

- **Testability**: Mock implementations for unit tests
- **Flexibility**: Swap storage without code rewrites
- **Scalability**: Add caching, sharding, etc. as decorators
- **Maintainability**: Clear separation of concerns
- **Extensibility**: Support multiple databases simultaneously

## SOLID Principles at Work

This design demonstrates four SOLID principles:

### S - Single Responsibility
Repository handles only data access
Services handle only business logic
Controllers handle only HTTP

### O - Open/Closed
Open for extension: Add new repository implementations
Closed for modification: Existing code untouched

### L - Liskov Substitution
Any `IRepoTaskItem` implementation can replace another
Controllers don't need to know which one

### D - Dependency Inversion
Controllers depend on `IRepoTaskItem` (abstraction)
Not on `RepoTaskItem` (concrete class)
Concrete classes depend on the interface

### (I - Interface Segregation)
`IRepoTaskItem` is small, focused
Controllers use only the methods they need

## Comparison: Before vs After

### Before (Tightly Coupled)

```csharp
// SQLiteRepository.cs
public class TaskService
{
    public void GetAllTasks()
    {
        using var sqlite = new SqliteConnection(...);
        var cmd = new SqliteCommand("SELECT ...", sqlite);
        // ...
    }
}

// To switch to PostgreSQL: Rewrite this entire class
```

### After (Repository Pattern)

```csharp
// Program.cs - Only this changes
-   builder.Services.AddDbContext<AppDbContext>(options =>
-       options.UseSqlite("..."));
+   builder.Services.AddDbContext<AppDbContext>(options =>
+       options.UseNpgsql(connectionString));

// Everything else: Unchanged
```

## How This Proves Itself

With this task, you'll see:

1. ? SQLite version works (Week 2)
2. ? PostgreSQL version works with same API
3. ? No controller changes
4. ? No route changes
5. ? No service changes
6. ? Only dependency injection + one NuGet package swap

**That's the architecture proving itself.**

## Further Reading

- [Martin Fowler: Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Dependency Injection in .NET](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection)
- [Entity Framework Core with Multiple Databases](https://learn.microsoft.com/ef/core/providers/)

