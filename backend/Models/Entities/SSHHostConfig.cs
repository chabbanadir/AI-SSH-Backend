using Backend.Models.Communs;
using Backend.Models.Entities;

namespace Backend.Models.Entities
{
    public class SSHHostConfig : AuditableEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Hostname { get; set; } = default!;
        public int Port { get; set; }
        public string Username { get; set; } = default!;
        public string AuthType { get; set; } = default!;
        public string? PasswordOrKeyPath { get; set; }
        public string UserId { get; set; } = default!;
        public AppUser User { get; set; } = default!;
        public bool SSHDefaultConfig { get; set; } // Example property
        public virtual ICollection<SSHSession> SSHSessions { get; set; } = new List<SSHSession>();
        public virtual ICollection<UserPreferences> DefaultPrefs { get; set; } = new List<UserPreferences>();
    }
}
