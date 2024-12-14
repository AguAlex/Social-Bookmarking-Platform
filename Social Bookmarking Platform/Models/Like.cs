using Social_Bookmarking_Platform.Data.Migrations;

namespace Social_Bookmarking_Platform.Models
{
    public class Like
    {

        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? BookmarkId { get; set; }
        public DateTime? DateLiked { get; set; }

        // Navigație la User și Bookmark
        public ApplicationUser? User { get; set; }
        public Bookmark? Bookmark { get; set; }
    }
}
