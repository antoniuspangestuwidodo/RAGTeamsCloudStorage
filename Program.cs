using Microsoft.AspNetCore.Builder;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using EchoBot.Bots;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddHttpClient<IRagService, RagService>();
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
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