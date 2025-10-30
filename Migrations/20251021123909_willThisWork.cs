using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movies.Migrations
{
    /// <inheritdoc />
    public partial class willThisWork : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReactionId",
                table: "ReviewReactions",
                newName: "ReviewReactionId");

            migrationBuilder.RenameColumn(
                name: "ReviewId",
                table: "MovieReviews",
                newName: "MovieReviewId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ReviewReactions",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "MovieReviews",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "MovieReviews",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2000);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReactions_UserId",
                table: "ReviewReactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MovieReviews_UserId",
                table: "MovieReviews",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovieReviews_AspNetUsers_UserId",
                table: "MovieReviews",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewReactions_AspNetUsers_UserId",
                table: "ReviewReactions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovieReviews_AspNetUsers_UserId",
                table: "MovieReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewReactions_AspNetUsers_UserId",
                table: "ReviewReactions");

            migrationBuilder.DropIndex(
                name: "IX_ReviewReactions_UserId",
                table: "ReviewReactions");

            migrationBuilder.DropIndex(
                name: "IX_MovieReviews_UserId",
                table: "MovieReviews");

            migrationBuilder.RenameColumn(
                name: "ReviewReactionId",
                table: "ReviewReactions",
                newName: "ReactionId");

            migrationBuilder.RenameColumn(
                name: "MovieReviewId",
                table: "MovieReviews",
                newName: "ReviewId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "ReviewReactions",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "MovieReviews",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "MovieReviews",
                type: "TEXT",
                maxLength: 2000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
