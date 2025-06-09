using Microsoft.AspNetCore.Builder;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using EchoBot.Bots;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "80";
Console.WriteLine("✅ Using PORT: " + port);
builder.WebHost.UseUrls($"http://*:{port}");

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
builder.Services.AddSingleton<AuthenticationConfiguration>();

// Registration for BotFrameworkAuthentication (from appsettings or env)
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Register for User State
builder.Services.AddSingleton<IStorage, MemoryStorage>();
builder.Services.AddSingleton<UserState>();
builder.Services.AddSingleton<IUserMemoryStore, UserMemoryStore>();

// Register the adapter via factory so we can call the correct overload
builder.Services.AddSingleton<IBotFrameworkHttpAdapter>(sp =>
{
    var credProv   = sp.GetRequiredService<ICredentialProvider>();
    var authConfig = sp.GetRequiredService<AuthenticationConfiguration>();
    var logger     = sp.GetRequiredService<ILogger<BotFrameworkHttpAdapter>>();

    // Call available overload : credentialProvider + authConfig
    var adapter = new BotFrameworkHttpAdapter(credProv, authConfig);

    // Apply OnTurnError with logger
    adapter.OnTurnError = async (turnContext, exception) =>
    {
        logger.LogError(exception, "❌ Bot error:");
        await turnContext.SendActivityAsync("Sorry, an error occured in bot.");
    };

    return adapter;
});

//Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
          .AllowAnyOrigin()      // or .WithOrigins("http://localhost") for specific origin
          .AllowAnyMethod()      // allowing OPTIONS, POST, etc
          .AllowAnyHeader();     // allowing header like Content-Type
    });
});

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddHttpClient<IRagService, RagService>();
builder.Services.AddSingleton<IBot, RagBot>();
builder.Services.AddSingleton<IUserMemoryStore, UserMemoryStore>();
builder.Services.AddSingleton<IDocumentStore, DocumentStore>();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<IDocumentFetcher, DocumentFetcher>();
builder.Services.AddHostedService<KeepAliveService>();

var app = builder.Build();

app.UseCors("AllowAll");

// Logging when the application stopped
app.Lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine("🛑 Application is stopping...");
});

app.Lifetime.ApplicationStopped.Register(() =>
{
    Console.WriteLine("❌ Application has stopped.");
});

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

Console.WriteLine("🚀 Running app...");
app.Run();
Console.WriteLine(">>> app.Run() finished");