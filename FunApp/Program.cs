using Microsoft.AspNetCore.SignalR;
using FunApp.Hubs;
using FunApp.Services;
using FunApp.Data;
using FunApp.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
var runningInContainer = !string.IsNullOrEmpty(port);
if (runningInContainer)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddSingleton<QuizService>();

var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=quiz.db";
builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddSingleton<PersistentQuizService>();

if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(serverOptions => { serverOptions.AddServerHeader = false; });
}

var app = builder.Build();

// Ensure DB exists and schema is healthy; if not, drop and recreate; then seed
using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var db = dbFactory.CreateDbContext();

    db.Database.EnsureCreated();

    bool schemaOk = true;
    try
    {
        // Check if Questions table exists by reading from it
        _ = db.Questions.Take(1).Any();
    }
    catch
    {
        schemaOk = false;
    }

    if (!schemaOk)
    {
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }

    if (!db.Questions.Any())
    {
        db.Questions.AddRange(new[]
        {
            new Question { Id = 1, Text = "What's your favorite color?", GameMode = GameMode.Individual },
            new Question { Id = 2, Text = "If you could be any animal, what would you be?", GameMode = GameMode.Individual },
            new Question { Id = 3, Text = "What's your dream vacation destination?", GameMode = GameMode.Individual },
            new Question { Id = 4, Text = "What superpower would you choose?", GameMode = GameMode.Individual },
            new Question { Id = 5, Text = "What's your favorite food?", GameMode = GameMode.Individual },
            new Question { Id = 6, Text = "How did you two meet?", GameMode = GameMode.Couple },
            new Question { Id = 7, Text = "What's your favorite memory together?", GameMode = GameMode.Couple },
            new Question { Id = 8, Text = "What do you love most about your partner?", GameMode = GameMode.Couple }
        });
        db.SaveChanges();
    }
}

// Log startup info
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting FunApp. RunningInContainer={runningInContainer}, PORT={port}", runningInContainer, port ?? "(none)");

if (!builder.Environment.IsDevelopment() && !runningInContainer)
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (!runningInContainer && !builder.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});

app.UseRouting();

app.MapRazorPages();
app.MapHub<QuizHub>("/quizHub");
app.MapGet("/health", () => Results.Ok("OK"));

app.Run();

