using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Logging;

public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
{
    public AdapterWithErrorHandler(ILogger<BotFrameworkHttpAdapter> logger)
        : base()
    {
        OnTurnError = async (context, exception) =>
        {            
            // Logging error ke console/log cloud
            logger.LogError(exception, "Bot Error: {Message}", exception.Message);

            // Send error message to User
            await context.SendActivityAsync("Sorry, an error occured in bot.");
            await context.SendActivityAsync($"Error Detail: {exception.Message}");
        };
    }
}