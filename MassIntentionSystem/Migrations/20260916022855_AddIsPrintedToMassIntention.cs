using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MassIntentionSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPrintedToMassIntention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPrinted",
                table: "MassIntentions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PrintedAt",
                table: "MassIntentions",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPrinted",
                table: "MassIntentions");

            migrationBuilder.DropColumn(
                name: "PrintedAt",
                table: "MassIntentions");
        }
    }
}
