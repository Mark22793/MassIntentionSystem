using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MassIntentionSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddPriestNameToMassIntention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PriestName",
                table: "MassIntentions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriestName",
                table: "MassIntentions");
        }
    }
}
