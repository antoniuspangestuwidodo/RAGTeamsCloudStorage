using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Logging;

public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
{
    public AdapterWithErrorHandler(ILogger<BotFrameworkHttpAdapter> logger)
        : base()
    {
        OnTurnError = async (context, exception) =>
        {
            // logger.LogError(exception, $"[OnTurnError] unhandled error : {exception.Message}");
            // await context.SendActivityAsync("There is an error, please try again.");

            // Logging error ke console/log cloud
            logger.LogError(exception, "Bot Error: {Message}", exception.Message);

            // Kirim pesan error ke user
            await context.SendActivityAsync("Maaf, terjadi kesalahan pada bot.");
            await context.SendActivityAsync($"Error Detail: {exception.Message}");
        };
    }
}