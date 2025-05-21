using System.Collections.Generic;
using System.Threading.Tasks;

namespace EchoBot.Services
{
    public interface IRagService
    {
        Task<string> GetAnswerAsync(string question, List<string> documents);
    }
}