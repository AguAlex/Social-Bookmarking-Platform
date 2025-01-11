using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace Social_Bookmarking_Platform.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Comment>? Comments { get; set; }

        public virtual ICollection<Bookmark>? Bookmarks { get; set; }

        public ICollection<Board> Boards { get; set; } = new List<Board>();

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? ProfileImage { get; set; } = "/images/unknown.jpg";

        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }
    }
}
