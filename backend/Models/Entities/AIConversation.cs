using Backend.Models.Communs;
using Backend.Models.Entities;

namespace Backend.Models.Entities
{
    public class AIConversation : AuditableEntity
    {
      public string Id { get; set; } = Guid.NewGuid().ToString();
      public string Topic { get; set; } = "Default Topic"; // You can customize this
      public DateTime StartedAt { get; set; } = DateTime.UtcNow;
      public DateTime? EndedAt { get; set; }
      public string UserId { get; set; } = default!;
      public AppUser User { get; set; } = default!;
      public string? SSHSessionId { get; set; }
      public SSHSession? LinkedSSHSession { get; set; }
      public virtual ICollection<AIMessage> AIMessages { get; set; } = new List<AIMessage>();
    }
}
