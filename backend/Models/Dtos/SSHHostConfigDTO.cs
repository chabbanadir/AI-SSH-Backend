// Backend/Models/Commons/SSHHostConfigDTO.cs
namespace Backend.Models.Dtos
{
        public class SSHHostConfigDTO
    {
        public string Id { get; set; } = default!;
        public string Hostname { get; set; } = default!;
        public int Port { get; set; }
        public string Username { get; set; } = default!;
        public string AuthType { get; set; } = default!;
        public string? PasswordOrKeyPath { get; set; }
        public string UserId { get; set; } = default!;
        public bool SSHDefaultConfig { get; set; }
    }
}
