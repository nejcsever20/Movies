using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movies.Migrations
{
    /// <inheritdoc />
    public partial class plzWork : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReviewReactions_MovieReviews_ReviewId",
                table: "ReviewReactions");

            migrationBuilder.RenameColumn(
                name: "ReviewId",
                table: "ReviewReactions",
                newName: "MovieReviewId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewReactions_ReviewId",
                table: "ReviewReactions",
                newName: "IX_ReviewReactions_MovieReviewId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewReactions_MovieReviews_MovieReviewId",
                table: "ReviewReactions",
                column: "MovieReviewId",
                principalTable: "MovieReviews",
                principalColumn: "MovieReviewId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReviewReactions_MovieReviews_MovieReviewId",
                table: "ReviewReactions");

            migrationBuilder.RenameColumn(
                name: "MovieReviewId",
                table: "ReviewReactions",
                newName: "ReviewId");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewReactions_MovieReviewId",
                table: "ReviewReactions",
                newName: "IX_ReviewReactions_ReviewId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewReactions_MovieReviews_ReviewId",
                table: "ReviewReactions",
                column: "ReviewId",
                principalTable: "MovieReviews",
                principalColumn: "MovieReviewId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
