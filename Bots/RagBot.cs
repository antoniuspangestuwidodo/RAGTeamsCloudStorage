using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace EchoBot.Bots
{
    public class RagBot : ActivityHandler
    {
        private readonly IRagService _ragService;
        private readonly IUserMemoryStore _memoryStore;
        private readonly HttpClient _httpClient;
        private readonly IDocumentFetcher _documentFetcher;
        private readonly string _documentURL;
        
        private readonly UserState _userState;
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;

        public RagBot(IRagService ragService,
            IUserMemoryStore memoryStore,
            IHttpClientFactory httpClientFactory,
            IDocumentFetcher documentFetcher,
            UserState userState)
        {
            _ragService = ragService;
            _memoryStore = memoryStore;
            _httpClient = httpClientFactory.CreateClient();
            _documentFetcher = documentFetcher;
            _documentURL = Environment.GetEnvironmentVariable("HF_DATASET_URL");
            _userState = userState;
            _userProfileAccessor = _userState.CreateProperty<UserProfile>("UserProfile");
        }

        protected override async Task OnMessageActivityAsync(
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var userProfile = await _userProfileAccessor.GetAsync(turnContext, () => new UserProfile(), cancellationToken);
            var userInput = turnContext.Activity.Text?.Trim();

            if (string.IsNullOrWhiteSpace(userInput))
            {
                await turnContext.SendActivityAsync("Please ask your question.", cancellationToken: cancellationToken);

                return;
            }

            if (userInput?.ToLower().StartsWith("my name is") == true)
            {
                userProfile.Name = userInput.Substring(11); // ambil nama
                await turnContext.SendActivityAsync($"Nice to meet you, {userProfile.Name}!", cancellationToken: cancellationToken);
            }
            else if (!string.IsNullOrEmpty(userProfile.Name))
            {
                await turnContext.SendActivityAsync($"Hi again, {userProfile.Name}! You asked: '{userInput}'", cancellationToken: cancellationToken);
                // RAG Process
                // 1. Get document from cloud (Hugging Face Dataset)            
                var docUrl = _documentURL;
                var context = await _documentFetcher.LoadFromUrlAsync(docUrl);

                // 2. Send to RAG Service
                var answer = await _ragService.GetAnswerAsync(context, userInput);

                // 3. Send answer to user
                await turnContext.SendActivityAsync(MessageFactory.Text(answer), cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync("Hi! What's your name?", cancellationToken: cancellationToken);
            }            

            // Save state
            await _userState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
        }
    }
}