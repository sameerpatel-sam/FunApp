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
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = false; // Reduce bandwidth
    options.KeepAliveInterval = TimeSpan.FromSeconds(30); // Longer intervals
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.MaximumReceiveMessageSize = 32 * 1024; // Limit message size
});
builder.Services.AddSingleton<QuizService>();

var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=quiz.db";
builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddSingleton<PersistentQuizService>();

if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(serverOptions => { serverOptions.AddServerHeader = false; });
}

var app = builder.Build();

// Ensure DB exists and schema is healthy; if not, recreate (no in-memory seeding now)
using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var db = dbFactory.CreateDbContext();

    db.Database.EnsureCreated();

    bool schemaOk = true;
    try { _ = db.Questions.Take(1).Any(); } catch { schemaOk = false; }
    if (!schemaOk)
    {
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
    }
    var individualCount = db.Questions.Count(q => q.GameMode == GameMode.Individual);
    var coupleCount = db.Questions.Count(q => q.GameMode == GameMode.Couple);
    app.Logger.LogInformation("DB question counts: Individual={IndividualCount}, Couple={CoupleCount}", individualCount, coupleCount);
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

