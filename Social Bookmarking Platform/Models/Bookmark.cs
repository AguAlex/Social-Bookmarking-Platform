using Microsoft.AspNetCore.Mvc.Rendering;
using Social_Bookmarking_Platform.Data.Migrations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social_Bookmarking_Platform.Models
{
    public class Bookmark
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [StringLength(100, ErrorMessage = "Titlul nu poate avea mai mult de 100 de caractere")]
        public string? Title { get; set; }
        public DateTime Date { get; set; }

        public int LikesCnt { get; set; } = 0;
        public string? Content { get; set; }


        // Adaugam un string unde vom salva calea imaginii pentru articol
        public string? Image{ get; set;  }

            [Required(ErrorMessage = "Categoria este obligatorie")]
        public int? CategoryId { get; set; }

        public string? UserId { get; set; }

        public virtual Category? Category { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<Comment>? Comments { get; set; }

        public virtual ICollection<BookmarkBoard>? BookmarkBoards { get; set; }

        public virtual ICollection<Like>? Likes { get; set; } = new List<Like>();


        [NotMapped]
        public IEnumerable<SelectListItem>? Categ { get; set; }
    }
}
