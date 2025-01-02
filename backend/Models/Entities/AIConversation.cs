using Backend.Models.Communs;
using Backend.Models.Entities;

namespace Backend.Models.Entities
{
    public class AIConversation : AuditableEntity
    {
      public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Topic { get; set; } = default!; // Added
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string UserId { get; set; } = default!;
    public AppUser User { get; set; } = default!;
    public string? LinkedSSHSessionId { get; set; } // Renamed
    public SSHSession? LinkedSSHSession { get; set; } // Renamed
    public virtual ICollection<AIMessage> AIMessages { get; set; } = new List<AIMessage>();
    }
}
