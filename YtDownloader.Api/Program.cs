using Microsoft.Extensions.FileProviders;
using YtDownloader.Database;
using YtDownloader.Core;
using FluentMigrator.Runner;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Add services to the container.
builder.Services.AddYtDownloaderServices();
builder.Services.AddYtDownloaderDatabase();

builder.Services.AddControllers();

var app = builder.Build();

using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
var runner = serviceScope.ServiceProvider.GetRequiredService<IMigrationRunner>();
runner.MigrateUp();

app.UseCors("AllowAll");
app.UseHttpsRedirection();

var staticFilesPath = Path.Combine(builder.Environment.ContentRootPath, "static");

if (Directory.Exists(staticFilesPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "static")),
        RequestPath = ""
    });
}
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
    context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
    context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization");
    await next();
});

app.MapControllers();

// Fallback for SPA
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 404 && !Path.HasExtension(context.Request.Path.Value))
    {
        context.Request.Path = "/index.html";
        await next();
    }
});

app.Run();