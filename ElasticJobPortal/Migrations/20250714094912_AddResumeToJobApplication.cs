using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElasticJobPortal.Migrations
{
    /// <inheritdoc />
    public partial class AddResumeToJobApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResumePath",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResumePath",
                table: "JobApplications");
        }
    }
}
