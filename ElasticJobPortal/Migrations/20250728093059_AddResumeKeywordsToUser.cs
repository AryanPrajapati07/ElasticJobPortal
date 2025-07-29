using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElasticJobPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddResumeKeywordsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResumeKeywords",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResumeKeywords",
                table: "AspNetUsers");
        }
    }
}
