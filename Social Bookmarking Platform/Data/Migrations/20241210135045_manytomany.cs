using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social_Bookmarking_Platform.Data.Migrations
{
    /// <inheritdoc />
    public partial class manytomany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BookmarkBoards",
                table: "BookmarkBoards");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookmarkBoards",
                table: "BookmarkBoards",
                columns: new[] { "Id", "BookmarkId", "BoardId" });

            migrationBuilder.CreateIndex(
                name: "IX_BookmarkBoards_BookmarkId",
                table: "BookmarkBoards",
                column: "BookmarkId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BookmarkBoards",
                table: "BookmarkBoards");

            migrationBuilder.DropIndex(
                name: "IX_BookmarkBoards_BookmarkId",
                table: "BookmarkBoards");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BookmarkBoards",
                table: "BookmarkBoards",
                columns: new[] { "BookmarkId", "BoardId" });
        }
    }
}
