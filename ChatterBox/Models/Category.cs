using System.ComponentModel.DataAnnotations;

namespace ChatterBox.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Categories are required to have a title!")]
        [MinLength(3, ErrorMessage ="Category title must contain at least 3 characters!")]
        [MaxLength(50, ErrorMessage = "Category title must contain at most 50 characters!")]
        public string? Title { get; set; }

        public virtual ICollection<Channel>? Channels { get; set; }
    }
}
