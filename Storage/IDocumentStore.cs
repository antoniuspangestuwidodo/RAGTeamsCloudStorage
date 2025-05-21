using System.Collections.Concurrent;
using System.Collections.Generic;

public interface IDocumentStore
{
    void AddDocument(string userId, string content);
    List<string> GetDocuments(string userId);
}

public class DocumentStore : IDocumentStore
{
    private readonly ConcurrentDictionary<string, List<string>> _userDocuments = new();

    public void AddDocument(string userId, string content)
    {
        _userDocuments.AddOrUpdate(userId,
            new List<string> { content },
            (key, existingList) =>
            {
                existingList.Add(content);
                return existingList;
            });
    }

    public List<string> GetDocuments(string userId)
    {
        if (_userDocuments.TryGetValue(userId, out var docs))
            return docs;
        return new List<string>();
    }
}