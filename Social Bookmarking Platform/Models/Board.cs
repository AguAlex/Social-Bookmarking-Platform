using System.ComponentModel.DataAnnotations;

namespace Social_Bookmarking_Platform.Models
{
    public class Board
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titulul board-ului este obligatoriu")]
        public string Title { get; set; }

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<BookmarkBoard>? BookmarkBoards { get; set; }
    }
}
