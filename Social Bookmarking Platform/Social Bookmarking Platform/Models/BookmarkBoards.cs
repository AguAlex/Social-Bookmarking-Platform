using System.ComponentModel.DataAnnotations.Schema;

namespace Social_Bookmarking_Platform.Models
{
        public class BookmarkBoard
        {
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }
            public int? BookmarkId { get; set; }
            public int? BoardId { get; set; }

            public virtual Bookmark? Bookmark { get; set; }
            public virtual Board? Board { get; set; }

            public DateTime BoardDate { get; set; }
        }
    
}
