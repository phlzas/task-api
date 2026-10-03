using System.Text.Json.Serialization;
using tasks.Entitys;
using tasks.Reposetry.IRepos;
using tasks.Reposetry.Repos;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=tasks.db"));

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.UnmappedMemberHandling =
        JsonUnmappedMemberHandling.Disallow);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    // The csproj's GenerateDocumentationFile emits the XML, but Swashbuckle
    // ignores it unless pointed at the file here. Both halves are required.
    var xml = Path.Combine(AppContext.BaseDirectory, "tasks.xml");
    if (File.Exists(xml))
    {
        o.IncludeXmlComments(xml);
    }
});
builder.Services.AddScoped<IRepoTaskItem, RepoTaskItem>();

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();

    // Insert sample data if table is empty
    if (!dbContext.TaskItems.Any())
    {
        dbContext.TaskItems.AddRange(
            new tasks.Models.Models.TaskItem { Title = "Learn C#", IsCompleted = false },
            new tasks.Models.Models.TaskItem { Title = "Build an API", IsCompleted = false },
            new tasks.Models.Models.TaskItem { Title = "Master SQLite", IsCompleted = false }
        );
        dbContext.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "docs/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = "docs";
        options.SwaggerEndpoint("/docs/v1/swagger.json", "Tasks API v1");
    });
}

// Disabled: with both http and https bound, this 307-redirects every plain
// http request, so the documented curl commands in the README return a
// redirect instead of 200. Re-enable it once this API is deployed somewhere
// that actually needs TLS termination.
app.UseAuthorization();

app.MapControllers();

app.Run();
