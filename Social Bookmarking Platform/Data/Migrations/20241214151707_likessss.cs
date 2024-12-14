using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social_Bookmarking_Platform.Data.Migrations
{
    /// <inheritdoc />
    public partial class likessss : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LikesCnt",
                table: "Bookmarks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LikesCnt",
                table: "Bookmarks");
        }
    }
}
