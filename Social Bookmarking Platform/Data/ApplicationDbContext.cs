using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Social_Bookmarking_Platform.Models;

namespace Social_Bookmarking_Platform.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>
        options)
        : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<BookmarkBoard> BookmarkBoards { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // definirea relatiei many-to-many dintre Article si Bookmark

            base.OnModelCreating(modelBuilder);

            // definire primary key compus
            modelBuilder.Entity<BookmarkBoard>()
                .HasKey(ab => new { ab.Id, ab.BookmarkId, ab.BoardId });


            // definire relatii cu modelele Bookmark si Article (FK)

            modelBuilder.Entity<BookmarkBoard>()
                .HasOne(ab => ab.Bookmark)
                .WithMany(ab => ab.BookmarkBoards)
                .HasForeignKey(ab => ab.BookmarkId);

            modelBuilder.Entity<BookmarkBoard>()
                .HasOne(ab => ab.Board)
                .WithMany(ab => ab.BookmarkBoards)
                .HasForeignKey(ab => ab.BoardId);
        }

    }
}
