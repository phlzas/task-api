using System.Text.Json.Serialization;
using tasks.Reposetry.IRepos;
using tasks.Reposetry.Repos;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.UnmappedMemberHandling =
        JsonUnmappedMemberHandling.Disallow);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IRepoTaskItem, RepoTaskItem>();

var app = builder.Build();

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
