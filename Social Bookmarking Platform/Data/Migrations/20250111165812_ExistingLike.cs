using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social_Bookmarking_Platform.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExistingLike : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ExistingLike",
                table: "Bookmarks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExistingLike",
                table: "Bookmarks");
        }
    }
}
