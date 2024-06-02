using Microsoft.AspNetCore.Identity;

namespace IdentityManagerServerApi.Data.Models
{
    public class ApplicationUser :IdentityUser
    {
        public string? FullName { get; set; }
    }
}
