using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Respondr.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ResourceRequestDecisionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CancelledAt",
                schema: "resources",
                table: "ResourceRequests",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecisionNotes",
                schema: "resources",
                table: "ResourceRequests",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledAt",
                schema: "resources",
                table: "ResourceRequests");

            migrationBuilder.DropColumn(
                name: "DecisionNotes",
                schema: "resources",
                table: "ResourceRequests");
        }
    }
}
