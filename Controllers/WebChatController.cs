using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("api/webchat")]
public class WebChatController : ControllerBase
{
    private readonly IRagService _rag;

    private readonly IDocumentFetcher _documentFetcher;
    private readonly string _documentURL;
    private readonly IUserMemoryStore _memoryStore;

    public WebChatController(
        IRagService rag,
        IDocumentFetcher documentFetcher,
        IUserMemoryStore memoryStore)
    {
        _rag = rag;
        _documentFetcher = documentFetcher;
        _documentURL = Environment.GetEnvironmentVariable("HF_DATASET_URL");
        _memoryStore = memoryStore;
    }

    public class WebChatRequest
    {
        public string Question { get; set; }
        public string Context { get; set; } = "";
        public string UserId { get; set; }
        public string UserName { get; set; }
    }

    public class WebChatResponse
    {
        public string Answer { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] WebChatRequest req)
    {
        var docUrl = _documentURL;
        var context = await _documentFetcher.LoadFromUrlAsync(docUrl);

        // Check if the input contain "my name is"
        if (!string.IsNullOrWhiteSpace(req.Question) &&
            req.Question.ToLower().StartsWith("my name is"))
        {
            var name = req.Question.Substring(11).Trim();
            if (!string.IsNullOrEmpty(req.UserId) && !string.IsNullOrEmpty(name))
            {
                await _memoryStore.SaveUserNameAsync(req.UserId, name);
                return Ok(new { answer = $"Nice to meet you, {name}!" });
            }
        }

        // Get username if available
        var userName = await _memoryStore.GetUserNameAsync(req.UserId);
        var answer = await _rag.GetAnswerAsync(context, req.Question);

        // Personalize if name available
        if (!string.IsNullOrWhiteSpace(userName))
        {
            answer = $"Hi {userName}, here's the answer:\n{answer}";
        }

        return Ok(new { answer });
    }
}
