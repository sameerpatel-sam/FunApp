using Microsoft.AspNetCore.SignalR;
using FunApp.Hubs;
using FunApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Bind to host-provided port (e.g. Replit) when available
var port = Environment.GetEnvironmentVariable("PORT");
var runningInContainer = !string.IsNullOrEmpty(port);
if (runningInContainer)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

// Add services
builder.Services.AddRazorPages();
builder.Services.AddSignalR();
builder.Services.AddSingleton<QuizService>();

// Configure Kestrel if not running behind a reverse proxy
if (!builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(serverOptions =>
    {
        serverOptions.AddServerHeader = false;
    });
}

var app = builder.Build();

// Log startup info
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Starting FunApp. RunningInContainer={runningInContainer}, PORT={port}", runningInContainer, port ?? "(none)");

// Configure the HTTP request pipeline
if (!builder.Environment.IsDevelopment() && !runningInContainer)
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Do not force HTTPS when running in a container that only exposes HTTP
if (!runningInContainer)
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

// Add security headers
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

// Health endpoint for Replit / load balancers
app.MapGet("/health", () => Results.Ok("OK"));

app.Run();

