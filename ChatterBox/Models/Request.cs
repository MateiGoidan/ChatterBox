using System.ComponentModel.DataAnnotations;

namespace ChatterBox.Models
{
	public class Request
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string? RequestType { get; set; }

		[Required]
		public DateTime CreatedAt { get; set; }

		public DateTime ProcessedAt { get; set; }

		public ICollection<BindRequestChannelUser>? BindRequestChannelUsers { get; set; }
	}
}
