// Backend/Models/Commons/CreateSSHHostConfigDTO.cs
using System.ComponentModel.DataAnnotations;

namespace Backend.Models.Dtos
{
  public class CreateSSHHostConfigDTO
    {
        [Required]
        public string Hostname { get; set; } = default!;

        [Required]
        [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535.")]
        public int Port { get; set; }

        [Required]
        public string Username { get; set; } = default!;

        [Required]
        public string AuthType { get; set; } = default!; // e.g., "Password" or "Key"

        public string? PasswordOrKeyPath { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        [Required]
        public bool SSHDefaultConfig { get; set; } // Indicates if this is the default configuration
    }
}
