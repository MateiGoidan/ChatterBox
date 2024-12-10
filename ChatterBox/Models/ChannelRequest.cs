using System.ComponentModel.DataAnnotations;

namespace ChatterBox.Models
{
    public class ChannelRequest
    {
        [Key]
        public int Id { get; set; }

        public int ChannelId { get; set; }

        public string? RequesterId { get; set; }

        public string? TargetUserId{ get; set; }

        [Required]
        public string? RequestType { get; set; }

        [Required]
        public string? Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ProcessedAt { get; set; }

        public virtual Channel? Channel { get; set; }

        public virtual ApplicationUser? Requester { get; set; }

        public virtual ApplicationUser? TargetUser { get; set; }
    }
}
