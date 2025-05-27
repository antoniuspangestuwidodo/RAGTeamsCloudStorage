using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/webchat")]
public class WebChatController : ControllerBase
{
    private readonly IRagService _rag;

    public WebChatController(IRagService rag)
    {
        _rag = rag;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] WebChatRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Question))
            return BadRequest("Question is required.");

        var answer = await _rag.GetAnswerAsync(req.Context ?? "", req.Question);
        return Ok(new { answer });
    }
}
