using Microsoft.AspNetCore.Identity;

namespace ChatterBox.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<BindChannelUser>? BindChannelUsers { get; set; }

        public ICollection<Message>? Messages { get; set; }
    }
}
