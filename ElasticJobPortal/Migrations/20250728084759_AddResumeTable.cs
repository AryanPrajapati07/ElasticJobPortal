using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElasticJobPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddResumeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Resumes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobSeekerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentText = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resumes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Resumes");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Jobs");
        }
    }
}
