using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElasticJobPortal.Migrations
{
    /// <inheritdoc />
    public partial class UpdateJobModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Skills",
                table: "Jobs",
                newName: "SkillsCsv");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SkillsCsv",
                table: "Jobs",
                newName: "Skills");
        }
    }
}
