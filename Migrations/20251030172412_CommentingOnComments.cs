using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Movies.Migrations
{
    /// <inheritdoc />
    public partial class CommentingOnComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentCommentId",
                table: "MovieComments",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovieComments_ParentCommentId",
                table: "MovieComments",
                column: "ParentCommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_MovieComments_MovieComments_ParentCommentId",
                table: "MovieComments",
                column: "ParentCommentId",
                principalTable: "MovieComments",
                principalColumn: "MovieCommentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovieComments_MovieComments_ParentCommentId",
                table: "MovieComments");

            migrationBuilder.DropIndex(
                name: "IX_MovieComments_ParentCommentId",
                table: "MovieComments");

            migrationBuilder.DropColumn(
                name: "ParentCommentId",
                table: "MovieComments");
        }
    }
}
