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
            var userId = turnContext.Activity.From.Id;
            var userProfile = await _userProfileAccessor.GetAsync(turnContext, () => new UserProfile(), cancellationToken);
            var userInput = turnContext.Activity.Text?.Trim();

            if (string.IsNullOrWhiteSpace(userInput))
            {
                await turnContext.SendActivityAsync("Please enter a message.", cancellationToken: cancellationToken);

                return;
            }

            // If user says "my name is ..."
            if (userInput.ToLower().StartsWith("my name is"))
            {
                var name = userInput.Substring(11).Trim();
                userProfile.Name = name;

                // Optionally save to external memory store as well
                await _memoryStore.SaveUserNameAsync(userId, name);

                await turnContext.SendActivityAsync($"Nice to meet you, {name}!", cancellationToken: cancellationToken);
                await _userProfileAccessor.SetAsync(turnContext, userProfile, cancellationToken);
                await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
                return;
            }

            // Retrieve name if exists (either from Bot State or external store)
            if (string.IsNullOrEmpty(userProfile.Name))
            {
                var nameFromStore = await _memoryStore.GetUserNameAsync(userId);
                if (!string.IsNullOrEmpty(nameFromStore))
                {
                    userProfile.Name = nameFromStore;
                }
            }
            
            // Load document context
            var context = await _documentFetcher.LoadFromUrlAsync(_documentURL);

            // Ask the RAG service
            var answer = await _ragService.GetAnswerAsync(context, userInput);

            // Customize response if name exists
            if (!string.IsNullOrEmpty(userProfile.Name))
            {
                answer = $"Hi {userProfile.Name}, here's the answer:\n{answer}";
            }

            await turnContext.SendActivityAsync(MessageFactory.Text(answer), cancellationToken);
        }
    }
}