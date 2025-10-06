using Auth.Domain.Models.Sessions;
using Microsoft.AspNetCore.Identity;


namespace Auth.Domain.Models.Users;

public class AppUser : IdentityUser
{
    
    public DateTime CreatedOn { get; set; }

    public DateTime ModifiedOn { get; set; }

    public ICollection<UserSession> Sessions { get; set; } = new List<UserSession>();
}