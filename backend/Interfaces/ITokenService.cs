using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Backend.Models;
using Microsoft.Extensions.Options;

namespace Backend.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(AppUser user, IList<string> roles);
    }

  
}