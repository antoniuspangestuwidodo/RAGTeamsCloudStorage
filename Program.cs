using Microsoft.AspNetCore.Builder;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using EchoBot.Bots;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// 1) Register credential + channel + authConfig
builder.Services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
builder.Services.AddSingleton<IChannelProvider, ConfigurationChannelProvider>();
builder.Services.AddSingleton<AuthenticationConfiguration>();

// 2) Register the adapter via factory so we can call the correct overload
builder.Services.AddSingleton<IBotFrameworkHttpAdapter>(sp =>
{
    var credProv    = sp.GetRequiredService<ICredentialProvider>();
    var authConfig  = sp.GetRequiredService<AuthenticationConfiguration>();
    var channelProv = sp.GetRequiredService<IChannelProvider>();
    var logger      = sp.GetRequiredService<ILogger<BotFrameworkHttpAdapter>>();

    // pass null for the optional RetryPolicy, HttpClient, and IMiddleware
    var adapter = new BotFrameworkHttpAdapter(
        credProv,
        authConfig,
        channelProv,
        connectorClientRetryPolicy: null,
        customHttpClient: null,
        middleware: null,
        logger: logger
    );

    adapter.OnTurnError = async (turnContext, exception) =>
    {
        await turnContext.SendActivityAsync("Maaf, terjadi kesalahan pada bot.");
        await turnContext.SendActivityAsync(exception.Message);
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
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.Run();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); // important for activate /api/messages
});
