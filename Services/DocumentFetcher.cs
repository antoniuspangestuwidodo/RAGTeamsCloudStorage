using System.Net.Http;
using System.Threading.Tasks;


public interface IDocumentFetcher
{
    Task<string> LoadFromUrlAsync(string url);
}

public class DocumentFetcher : IDocumentFetcher
{
    private readonly HttpClient _httpClient;

    public DocumentFetcher(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> LoadFromUrlAsync(string url)
    {
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStringAsync();
    }
}
