using System.Collections.Generic;

public interface IUserMemoryStore
{
    void AddMessage(string userId, string message);
    string GetConversation(string userId);
    
    void AddDocument(string userId, string fileName, string content);
    List<string> GetDocuments(string userId);
}

public class UserMemoryStore : IUserMemoryStore
{    
    private readonly Dictionary<string, List<string>> _conversations = new();
    private readonly Dictionary<string, List<string>> _documents = new();

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
}