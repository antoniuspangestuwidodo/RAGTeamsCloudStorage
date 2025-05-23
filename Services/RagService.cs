using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public interface IRagService
{
    Task<string> GetAnswerAsync(string context, string question);
}

public class RagService : IRagService
{
    private readonly HttpClient _httpClient;
    private readonly string _huggingFaceApiKey;
    private readonly string _modelUrl;
    private readonly ILogger<RagService> _logger;

    public RagService(HttpClient httpClient, IConfiguration configuration, ILogger<RagService> logger)
    {
        // _huggingFaceApiKey = configuration["HuggingFace:ApiKey"];
        _huggingFaceApiKey = Environment.GetEnvironmentVariable("HF_API_KEY");
        if (string.IsNullOrEmpty(_huggingFaceApiKey))
            throw new Exception("Hugging Face API key is missing in environment variables.");
        
        _logger.LogInformation("🔍 HF_API_KEY = {ApiKey}", _huggingFaceApiKey ?? "<null>");

        _httpClient = httpClient;
        _logger = logger;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _huggingFaceApiKey);
        // _modelUrl = configuration["HuggingFace:ModelURL"];
        _modelUrl = Environment.GetEnvironmentVariable("HF_MODEL_URL");
        if (string.IsNullOrEmpty(_modelUrl))
            throw new Exception("Model URL is missing in environment variables.");

        _logger.LogInformation("🔍 HF_MODEL_URL = {ModelUrl}", _modelUrl ?? "<null>");

        var baseUrl = Environment.GetEnvironmentVariable("RAG_BASE_URL");
        if (string.IsNullOrEmpty(baseUrl))
            throw new Exception("RAG_BASE_URL is missing in environment variables.");        
        _httpClient.BaseAddress = new Uri(baseUrl);

        _logger.LogInformation("🔍 RAG_BASE_URL = {BaseUrl}", baseUrl ?? "<null>");
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