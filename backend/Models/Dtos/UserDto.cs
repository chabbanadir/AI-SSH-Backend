namespace Backend.Models.Dtos
{
    public class UserDto
    {
        public string UserId { get; set; }     // or 'Id' 
        public string UserName { get; set; }   // might be known as 'Username'
        public string? Email { get; set; } 
        public string SshClient {get ; set;} // only if you want to expose email
    }
}