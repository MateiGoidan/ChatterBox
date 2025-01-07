using Microsoft.AspNetCore.Identity;

namespace ChatterBox.Models
{
	public class ApplicationUser : IdentityUser
	{
		public ICollection<Message>? Messages { get; set; }

		public ICollection<BindChannelUser>? BindChannelUsers { get; set; }

		public ICollection<BindRequestChannelUser>? BindRequestChannelUsers { get; set; }
	}
}
