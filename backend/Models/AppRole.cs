    // Models/AppRole.cs
using Microsoft.AspNetCore.Identity;
using System;

namespace Backend.Models
{
    public class AppRole : IdentityRole<string>
    {
    public AppRole() : base() {
        Id = Guid.NewGuid().ToString(); // Ensure Id is set
        NormalizedName = Name?.ToUpper(); // Ensure NormalizedName is set

     }

    public AppRole(string roleName) : base(roleName)
    {
            Id = Guid.NewGuid().ToString(); // Ensure Id is set
            Name = roleName;
            NormalizedName = roleName.ToUpper();    }
            
    }
}
