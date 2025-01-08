// File: Services/GenerativeAI/IGenerativeAIService.cs
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Interfaces
{
    public interface IGenerativeAIService
    {
        Task<string> GenerateTextAsync(string prompt, CancellationToken cancellationToken);
    }
}
