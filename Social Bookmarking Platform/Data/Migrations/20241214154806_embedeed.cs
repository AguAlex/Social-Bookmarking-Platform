using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social_Bookmarking_Platform.Data.Migrations
{
    /// <inheritdoc />
    public partial class embedeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmbedContent",
                table: "Bookmarks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmbedContent",
                table: "Bookmarks");
        }
    }
}
