using Microsoft.AspNetCore.Identity;
using Backend.Models.Communs;
using Backend.Models.Entities;

namespace Backend.Models{

    public class AppUser : IdentityUser<string> {
        public AppUser()
        {
            Id = Guid.NewGuid().ToString(); // Generate a GUID string as the ID
            Created = DateTime.UtcNow;
            IsDeleted = false;
            // Initialize collections if necessary
            SSHHostConfigs = new List<SSHHostConfig>();
            SSHSessions = new List<SSHSession>();
            AIConversations = new List<AIConversation>();
        }

        public DateTime Created { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModified { get; set; }
        public string? LastModifiedBy { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation property for 1-to-1 with UserPreferences
        public virtual UserPreferences? UserPreferences { get; set; }

        // Navigation property for 1-to-many with SSHHostConfig
        public virtual ICollection<SSHHostConfig>? SSHHostConfigs { get; set; }

        // Navigation property for 1-to-many with SSHSession
        public virtual ICollection<SSHSession>? SSHSessions { get; set; }

        // Navigation property for 1-to-many with AIConversation
        public virtual ICollection<AIConversation>? AIConversations { get; set; }
    }
}