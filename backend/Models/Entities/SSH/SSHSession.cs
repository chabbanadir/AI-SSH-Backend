using Backend.Models.Communs;
using Backend.Models.Entities.SSH;
using Backend.Models.Entities.AI;
namespace Backend.Models.Entities.SSH
{
    public class SSHSession : AuditableEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime SessionStartTime { get; set; }
        public DateTime? SessionEndTime { get; set; }
        public string? InitialWorkingDirectory { get; set; }
        public string UserId { get; set; } = default!;
        public AppUser User { get; set; } = default!;
        public string SSHHostConfigId { get; set; } = default!;
        public SSHHostConfig SSHHostConfig { get; set; } = default!;
        public virtual ICollection<SSHCommand> SSHCommands { get; set; } = new List<SSHCommand>();
        public virtual ICollection<AIConversation> AIConversations { get; set; } = new List<AIConversation>();
    }
}
