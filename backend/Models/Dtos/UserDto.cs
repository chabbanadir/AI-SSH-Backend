namespace Backend.Models.Dtos
{
    public class UserDto
    {
        public string Id {get; set;}
        public string UserId { get; set; }     // or 'Id' 
        public string UserName { get; set; }   // might be known as 'Username'
        public string? Email { get; set; } 
        public IList<string> Roles { get; set; } = new List<string>();

    public UserDto()
    {
        Id = string.Empty;
        UserId = string.Empty;
        UserName = string.Empty;
        Email = string.Empty;
    }
    }
}