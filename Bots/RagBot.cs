using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace EchoBot.Bots
{
    public class RagBot : ActivityHandler
    {
        private readonly IRagService _ragService;
        private readonly IUserMemoryStore _memoryStore;
        private readonly HttpClient _httpClient;
        private readonly IDocumentFetcher _documentFetcher;
        private readonly string _documentURL;

        public RagBot(IRagService ragService,
            IUserMemoryStore memoryStore,
            IHttpClientFactory httpClientFactory,
            IDocumentFetcher documentFetcher,
            IConfiguration configuration)
        {
            _ragService = ragService;
            _memoryStore = memoryStore;
            _httpClient = httpClientFactory.CreateClient();
            _documentFetcher = documentFetcher;
            _documentURL = configuration["HuggingFace:DatasetURL"];
        }

        protected override async Task OnMessageActivityAsync(
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var userInput = turnContext.Activity.Text?.Trim();

            if (string.IsNullOrWhiteSpace(userInput))
            {
                await turnContext.SendActivityAsync("Please ask your question.", cancellationToken: cancellationToken);

                return;
            }

            // 1. Get document from cloud (Hugging Face Dataset)            
            var docUrl = _documentURL;
            var context = await _documentFetcher.LoadFromUrlAsync(docUrl);

            // 2. Send to RAG Service
            var answer = await _ragService.GetAnswerAsync(context, userInput);

            // 3. Send answer to user
            await turnContext.SendActivityAsync(MessageFactory.Text(answer), cancellationToken);
        }
    }
}