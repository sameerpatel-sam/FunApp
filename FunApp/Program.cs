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
    options.EnableDetailedErrors = builder.Environment.IsDevelopment(); // Enable in dev for debugging
    options.KeepAliveInterval = TimeSpan.FromSeconds(30); // Longer intervals
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.MaximumReceiveMessageSize = 32 * 1024; // Limit message size
});
builder.Services.AddSingleton<QuizService>(sp => 
    new QuizService(sp.GetRequiredService<ILogger<QuizService>>()));

var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=quiz.db";
builder.Services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddSingleton<PersistentQuizService>();

if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(serverOptions => { serverOptions.AddServerHeader = false; });
}

var app = builder.Build();

// Ensure DB exists and schema is healthy; if not, recreate
using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    
    bool needsRecreation = false;
    
    // Check if database needs to be created or recreated
    using (var db = dbFactory.CreateDbContext())
    {
        try
        {
            // First check: Can we connect and does Questions table exist?
            var canQuery = db.Database.CanConnect();
            if (canQuery)
            {
                _ = db.Questions.Take(1).Any();
            }
            else
            {
                needsRecreation = true;
            }
        }
        catch
        {
            // Questions table missing or database corrupt
            needsRecreation = true;
        }
        
        // Second check: Does CoupleScores table exist?
        if (!needsRecreation)
        {
            try
            {
                _ = db.CoupleScores.Take(1).Any();
            }
            catch
            {
                // CoupleScores table missing - need to recreate
                app.Logger.LogWarning("CoupleScores table not found. Database needs migration.");
                needsRecreation = true;
            }
        }
    } // Dispose db context here to release file locks
    
    // Recreate database if needed
    if (needsRecreation)
    {
        app.Logger.LogWarning("Database schema is outdated or incomplete. Recreating database...");
        
        // Small delay to ensure file locks are released
        System.Threading.Thread.Sleep(100);
        
        try
        {
            using (var db = dbFactory.CreateDbContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }
            app.Logger.LogInformation("✅ Database recreated successfully with CoupleScores table.");
        }
        catch (IOException ioEx) when (ioEx.Message.Contains("being used by another process"))
        {
            app.Logger.LogError("Database file is locked. Please close any applications accessing quiz.db and restart the app.");
            app.Logger.LogError("Alternative: Delete quiz.db manually and restart.");
            throw new InvalidOperationException(
                "Cannot recreate database - file is locked. Please:\n" +
                "1. Stop the application\n" +
                "2. Delete quiz.db, quiz.db-wal, and quiz.db-shm files\n" +
                "3. Restart the application", ioEx);
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Failed to recreate database.");
            throw;
        }
    }
    else
    {
        // Ensure database exists (no-op if already exists with correct schema)
        using (var db = dbFactory.CreateDbContext())
        {
            db.Database.EnsureCreated();
        }
    }
    
    // Get question counts
    using (var db = dbFactory.CreateDbContext())
    {
        var individualCount = db.Questions.Count(q => q.GameMode == GameMode.Individual);
        var coupleCount = db.Questions.Count(q => q.GameMode == GameMode.Couple);
        app.Logger.LogInformation("DB question counts: Individual={IndividualCount}, Couple={CoupleCount}", individualCount, coupleCount);
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

