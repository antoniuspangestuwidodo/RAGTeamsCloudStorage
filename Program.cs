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

// var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
// builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

builder.Services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
builder.Services.AddSingleton<AuthenticationConfiguration>();

// 1. Registrasi BotFrameworkAuthentication (dari appsettings or env)
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();


// 2) Register the adapter via factory so we can call the correct overload
builder.Services.AddSingleton<IBotFrameworkHttpAdapter>(sp =>
{
    var credProv   = sp.GetRequiredService<ICredentialProvider>();
    var authConfig = sp.GetRequiredService<AuthenticationConfiguration>();
    var logger     = sp.GetRequiredService<ILogger<BotFrameworkHttpAdapter>>();

    // Panggil overload yang tersedia: credentialProvider + authConfig
    var adapter = new BotFrameworkHttpAdapter(credProv, authConfig);

    // Pasang OnTurnError pakai logger
    adapter.OnTurnError = async (turnContext, exception) =>
    {
        logger.LogError(exception, "❌ Bot error:");
        await turnContext.SendActivityAsync("Sorry, an error occured in bot.");
    };

    return adapter;
});



builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddHttpClient<IRagService, RagService>();
// builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
builder.Services.AddSingleton<IBot, RagBot>();
builder.Services.AddSingleton<IUserMemoryStore, UserMemoryStore>();
builder.Services.AddSingleton<IDocumentStore, DocumentStore>();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<IDocumentFetcher, DocumentFetcher>();


var app = builder.Build();
app.UseRouting();

app.UseAuthorization();

app.MapControllers();

// app.Run();

Console.WriteLine(">>> Starting app.Run()");
await app.RunAsync();
Console.WriteLine(">>> app.Run() finished");


// app.UseEndpoints(endpoints =>
// {
//     endpoints.MapControllers(); // important for activate /api/messages
// });
