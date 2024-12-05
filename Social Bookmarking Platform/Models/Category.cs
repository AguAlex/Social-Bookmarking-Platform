using System.ComponentModel.DataAnnotations;

namespace Social_Bookmarking_Platform.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Numele categoriei este obligatoriu!")]
        public string Title { get; set; }
        public virtual ICollection<Bookmark>? Bookmarks { get; set; }
    }
}
