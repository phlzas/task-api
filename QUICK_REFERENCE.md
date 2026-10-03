# Quick Reference - SQLite Migration

## What Changed

### Before (In-Memory)
```csharp
// Storage: List<TaskItem> in memory
// Scope: Singleton
// Persistence: No
```

### After (SQLite)
```csharp
// Storage: SQLite database (tasks.db)
// Access: Entity Framework Core with DbContext
// Persistence: Yes - data survives restarts
```

## Running the Application

```bash
cd tasks
dotnet run
```

The application will:
1. Create `tasks.db` in the current directory (if it doesn't exist)
2. Create the `tasks` table (if it doesn't exist)
3. Insert 3 example tasks (only on first run)
4. Start the API on `http://localhost:5241`

## Testing CRUD Operations

### Create a task
```bash
curl -X POST http://localhost:5241/tasks \
  -H "Content-Type: application/json" \
  -d '{"title":"Buy milk"}'
```

### List all tasks
```bash
curl http://localhost:5241/tasks
```

### Get one task
```bash
curl http://localhost:5241/tasks/1
```

### Update a task
```bash
curl -X PUT http://localhost:5241/tasks/1 \
  -H "Content-Type: application/json" \
  -d '{"title":"Buy milk and cheese","isCompleted":true}'
```

### Delete a task
```bash
curl -X DELETE http://localhost:5241/tasks/1
```

## Database Management

### View the database

1. Download [DB Browser for SQLite](https://sqlitebrowser.org/)
2. Open `tasks.db`
3. Click "Browse Data" tab
4. Select "tasks" table

### Manual SQL queries in DB Browser

```sql
-- See all tasks
SELECT * FROM tasks;

-- See only completed tasks
SELECT * FROM tasks WHERE IsCompleted = 1;

-- Count tasks
SELECT COUNT(*) FROM tasks;

-- Make all tasks completed
UPDATE tasks SET IsCompleted = 1;

-- Delete completed tasks
DELETE FROM tasks WHERE IsCompleted = 1;
```

## Key Files

| File | Purpose |
|------|---------|
| `tasks.db` | SQLite database (auto-created) |
| `Program.cs` | Database initialization & DI setup |
| `Entitys/AppDbContext.cs` | Entity Framework DbContext |
| `Reposetry/Repos/RepoTaskItem.cs` | Data access layer |
| `Controllers/TasksController.cs` | API endpoints |

## Architecture

```
Request ? Controller ? Repository ? DbContext ? SQLite (tasks.db)
Response ? Controller ? Repository ? DbContext ? SQLite (tasks.db)
```

## Important Notes

- Database location: `tasks/tasks.db`
- Database is automatically created on first run
- Sample data (3 tasks) is only inserted once
- All CRUD operations persist to SQLite
- The API endpoints remain unchanged
- Data survives application restarts

