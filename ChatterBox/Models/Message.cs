using System.ComponentModel.DataAnnotations;

namespace ChatterBox.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [MinLength(1)]
        [MaxLength(100, ErrorMessage = "Message lengt reached the 100 character limit!")]
        public string? Content{ get; set; }


        [MaxLength(100, ErrorMessage = "Message file path reached the 100 character limit!")]
        public string? FilePath { get; set; }


        [MaxLength(100, ErrorMessage = "Message file type reached the 100 character limit!")]
        public string? FileType{ get; set; }

        [Required]
        public DateTime? Date { get; set; }

        [Required]
        public string? UserId { get; set; }

        [Required]
        public int ChannelId { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual Channel? Channel { get; set; }
    }
}
