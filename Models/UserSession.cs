using System.Collections.Generic;

namespace EchoBot.Models;

public class UserSession
{
    public string UserId { get; set; }
    public string Name { get; set; }
    public List<string> MessageHistory { get; set; } = new();
    public Dictionary<string, string> Documents { get; set; } = new(); // filename → content
}