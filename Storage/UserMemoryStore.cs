using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IUserMemoryStore
{
    void AddMessage(string userId, string message);
    string GetConversation(string userId);

    void AddDocument(string userId, string fileName, string content);
    List<string> GetDocuments(string userId);
    Task SaveUserNameAsync(string userId, string userName);
    Task<string> GetUserNameAsync(string userId);
}

public class UserMemoryStore : IUserMemoryStore
{
    private readonly Dictionary<string, List<string>> _conversations = new();
    private readonly Dictionary<string, List<string>> _documents = new();
    private readonly ConcurrentDictionary<string, string> _userNames = new();

    public void AddMessage(string userId, string message)
    {
        if (!_conversations.ContainsKey(userId))
            _conversations[userId] = new List<string>();

        _conversations[userId].Add(message);
    }

    public string GetConversation(string userId)
    {
        return _conversations.ContainsKey(userId)
            ? string.Join("\n", _conversations[userId])
            : string.Empty;
    }

    public void AddDocument(string userId, string fileName, string content)
    {
        if (!_documents.ContainsKey(userId))
            _documents[userId] = new List<string>();

        _documents[userId].Add($"[File: {fileName}]\n{content}");
    }

    public List<string> GetDocuments(string userId)
    {
        return _documents.ContainsKey(userId)
            ? _documents[userId]
            : new List<string>();
    }
    
    public Task SaveUserNameAsync(string userId, string userName)
    {
        _userNames[userId] = userName;
        return Task.CompletedTask;
    }

    public Task<string> GetUserNameAsync(string userId)
    {
        _userNames.TryGetValue(userId, out var name);
        return Task.FromResult(name);
    }
}