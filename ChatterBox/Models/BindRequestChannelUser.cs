using System.ComponentModel.DataAnnotations;

namespace ChatterBox.Models
{
	public class BindRequestChannelUser
	{
		public int ChannelId { get; set; }

		public required string UserId { get; set; }

		public int RequestId { get; set; }

		[Required]
		public string? Status { get; set; }

		public virtual ApplicationUser? User { get; set; }

		public virtual Channel? Channel { get; set; }

		public virtual Request? Request { get; set; }
	}
}
