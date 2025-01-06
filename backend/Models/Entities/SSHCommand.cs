using Backend.Models.Communs;
using Backend.Models.Entities;

namespace Backend.Models.Entities
{
    public class SSHCommand : AuditableEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string CommandText { get; set; } = default!;
        public string? Output { get; set; }
        public int? ExitCode { get; set; }
        public DateTime ExecutedAt { get; set; }
        public string SSHSessionId { get; set; } = default!;
        public SSHSession SSHSession { get; set; } = default!;
    }
}
