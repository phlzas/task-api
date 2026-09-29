using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using TaskApi.Models;
using TaskApi.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<TaskStore>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Task API", Version = "1.0" });
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "TaskApi.xml"), true);
});

// Model-binding and JSON-deserialization failures must surface as 400 with a JSON body,
// never as 422 and never as an empty response.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var message = context.ModelState.Values
            .SelectMany(entry => entry.Errors)
            .Select(error => error.ErrorMessage)
            .FirstOrDefault(error => !string.IsNullOrWhiteSpace(error))
            ?? "The request body could not be parsed.";

        return new BadRequestObjectResult(new ErrorResponse(message));
    };
});

var app = builder.Build();

app.UseSwagger(options => options.RouteTemplate = "docs/{documentName}/swagger.json");
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/docs/v1/swagger.json", "Task API v1");
    options.RoutePrefix = "docs";
    options.DocumentTitle = "Task API";
});

app.MapControllers();

app.Run();
