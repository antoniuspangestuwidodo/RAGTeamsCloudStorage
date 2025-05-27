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

    public WebChatController(IRagService rag, IDocumentFetcher documentFetcher)
    {
        _rag = rag;
        _documentFetcher = documentFetcher;
        _documentURL = Environment.GetEnvironmentVariable("HF_DATASET_URL");            
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] WebChatRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Question))
            return BadRequest("Question is required.");

        var docUrl = _documentURL;
        var context = await _documentFetcher.LoadFromUrlAsync(docUrl);

        var answer = await _rag.GetAnswerAsync(context, req.Question);
        return Ok(new { answer });
    }
}
