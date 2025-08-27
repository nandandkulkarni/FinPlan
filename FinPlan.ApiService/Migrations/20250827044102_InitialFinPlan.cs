using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinPlan.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class InitialFinPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "finplan");

            migrationBuilder.CreateTable(
                name: "FinPlan",
                schema: "finplan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserGuid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CalculatorType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinPlan", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinPlan",
                schema: "finplan");
        }
    }
}
