using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public interface IRagService
{
    Task<string> GetAnswerAsync(string context, string question);
}

public class RagService : IRagService
{
    private readonly HttpClient _httpClient;
    private readonly string _huggingFaceApiKey;
    private readonly string _modelUrl;

    public RagService(HttpClient httpClient, IConfiguration configuration)
    {
        // _huggingFaceApiKey = configuration["HuggingFace:ApiKey"];
        _huggingFaceApiKey = Environment.GetEnvironmentVariable("HF_API_KEY");
        _httpClient = httpClient;

        if (string.IsNullOrEmpty(_huggingFaceApiKey))
            throw new Exception("Hugging Face API key is missing in environment variables.");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _huggingFaceApiKey);
        _modelUrl = configuration["HuggingFace:ModelURL"];
    }

    public async Task<string> GetAnswerAsync(string documentContent, string userQuestion)
    {
        if (string.IsNullOrWhiteSpace(documentContent))
        {
            return "Sorry, the document is empty.";
        }

        var payload = new
        {
            inputs = new
            {
                question = userQuestion,
                context = documentContent
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_modelUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            return $"Sorry, failed to answer. ({(int)response.StatusCode})";
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();        
        using var doc = JsonDocument.Parse(jsonResponse);
        var answerData = doc.RootElement;

        if (!answerData.TryGetProperty("answer", out var answerElement) ||
            !answerData.TryGetProperty("score", out var scoreElement))
        {
            return "Sorry, the answer format is not recognized.";
        }

        var answer = answerElement.GetString();
        var score = scoreElement.GetDouble();

        return score > 0.01 && !string.IsNullOrWhiteSpace(answer)
            ? answer
            : "Sorry, no convincing answers found.";
    }
}