using Backend.Models.Communs;
using Backend.Models.Entities;

namespace Backend.Models.Entities.SSH
{
    public class UserPreferences : AuditableEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string? Language { get; set; }
        public string? Theme { get; set; }

        // New Properties
        public string? PreferenceToken { get; set; }
        public bool IsDefault { get; set; }
        // For the default SSH Host:
        public string? DefaultSSHHostId { get; set; }
        public SSHHostConfig? DefaultSSHHost { get; set; }

        // The one-to-one user link:
        public string UserId { get; set; } = default!;
        public AppUser User { get; set; } = default!;
    }
}
