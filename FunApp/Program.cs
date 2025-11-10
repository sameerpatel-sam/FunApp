using Microsoft.AspNetCore.SignalR;
using FunApp.Hubs;
using FunApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Bind to host-provided port (e.g. Replit) when available
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
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

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
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

app.Run();
