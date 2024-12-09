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

        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<Board> Boards { get; set; }
        public DbSet<BookmarkBoard> BookmarkBoards { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Comment> Comment { get; set; }
        protected override void OnModelCreating(ModelBuilder
        modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<BookmarkBoard>()
            .HasKey(ac => new { ac.BookmarkId, ac.BoardId });
            modelBuilder.Entity<BookmarkBoard>()
            .HasOne(ac => ac.Bookmark)
            .WithMany(ac => ac.BookmarkBoards)
            .HasForeignKey(ac => ac.BookmarkId);
            modelBuilder.Entity<BookmarkBoard>()
            .HasOne(ac => ac.Board)
            .WithMany(ac => ac.BookmarkBoards)
            .HasForeignKey(ac => ac.BoardId);
        }

    }
}
