-- Create tasks table
CREATE TABLE IF NOT EXISTS "TaskItems" (
    "Id" SERIAL PRIMARY KEY,
    "Title" VARCHAR(255) NOT NULL,
    "IsCompleted" BOOLEAN NOT NULL DEFAULT FALSE
);

-- Create index on IsCompleted for filtering
CREATE INDEX IF NOT EXISTS idx_task_items_is_completed ON "TaskItems"("IsCompleted");

-- Create index on Title for searching
CREATE INDEX IF NOT EXISTS idx_task_items_title ON "TaskItems"("Title");

-- Insert initial sample data if table is empty
INSERT INTO "TaskItems" ("Title", "IsCompleted")
SELECT 'Learn C#', FALSE WHERE NOT EXISTS (SELECT 1 FROM "TaskItems" LIMIT 1);

INSERT INTO "TaskItems" ("Title", "IsCompleted")
SELECT 'Build an API', FALSE WHERE NOT EXISTS (SELECT 1 FROM "TaskItems" WHERE "Title" = 'Build an API');

INSERT INTO "TaskItems" ("Title", "IsCompleted")
SELECT 'Master PostgreSQL', FALSE WHERE NOT EXISTS (SELECT 1 FROM "TaskItems" WHERE "Title" = 'Master PostgreSQL');
