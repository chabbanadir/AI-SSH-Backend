// Services/IAIService.cs
using System.Threading;
using System.Threading.Tasks;

namespace Backend.Interfaces{
    public interface IAIService
    {
        Task<string> GenerateContentAsync(string prompt, CancellationToken cancellationToken);
    }
}