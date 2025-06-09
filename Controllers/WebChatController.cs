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
        // if (string.IsNullOrWhiteSpace(req.Question))
        //     return BadRequest("Question is required.");

        // 1. Save userName if provided
        if (!string.IsNullOrWhiteSpace(req.UserId) && !string.IsNullOrWhiteSpace(req.UserName))
        {
            await _memoryStore.SaveUserNameAsync(req.UserId, req.UserName);
        }

        // 2. Get user name if available
        var name = string.Empty;
        if (!string.IsNullOrWhiteSpace(req.UserId))
        {
            name = await _memoryStore.GetUserNameAsync(req.UserId);
        }

        var docUrl = _documentURL;
        var context = await _documentFetcher.LoadFromUrlAsync(docUrl);

        var answer = await _rag.GetAnswerAsync(context, req.Question);

        // 🎯 Try to get username from memory
        var finalAnswer = !string.IsNullOrEmpty(name)
            ? $"{name}, {answer}"
            : answer;

        //return Ok(new { answer });
        return Ok(new WebChatResponse { Answer = finalAnswer });
    }
}
