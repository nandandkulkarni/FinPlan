using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinPlan.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddCityTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "finplan",
                table: "FinPlan",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                schema: "finplan",
                table: "FinPlan",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "finplan",
                table: "FinPlan",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "CityTemplates",
                schema: "finplan",
                columns: table => new
                {
                    CityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CostOfLivingIndex = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityTemplates", x => x.CityId);
                });

            migrationBuilder.CreateTable(
                name: "ContactMessages",
                schema: "finplan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserGuid = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SurveyResponses",
                schema: "finplan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserGuid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SurveyType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SurveyJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SurveyResponses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                schema: "finplan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserGuid = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Provider = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSignInAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserRegistration",
                schema: "finplan",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CookieGuid = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRegistration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DemographicProfiles",
                schema: "finplan",
                columns: table => new
                {
                    ProfileId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProfileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AgeMin = table.Column<int>(type: "int", nullable: false),
                    AgeMax = table.Column<int>(type: "int", nullable: false),
                    MaritalStatus = table.Column<int>(type: "int", nullable: false),
                    ChildrenCount = table.Column<int>(type: "int", nullable: false),
                    ChildrenAgesJSON = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SampleExpensesJSON = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DemographicProfiles", x => x.ProfileId);
                    table.ForeignKey(
                        name: "FK_DemographicProfiles_CityTemplates_CityId",
                        column: x => x.CityId,
                        principalSchema: "finplan",
                        principalTable: "CityTemplates",
                        principalColumn: "CityId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserDemographics",
                schema: "finplan",
                columns: table => new
                {
                    UserGuid = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Age = table.Column<int>(type: "int", nullable: false),
                    MaritalStatus = table.Column<int>(type: "int", nullable: false),
                    ChildrenAgesJSON = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferredCurrency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SelectedCityId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDemographics", x => x.UserGuid);
                    table.ForeignKey(
                        name: "FK_UserDemographics_CityTemplates_SelectedCityId",
                        column: x => x.SelectedCityId,
                        principalSchema: "finplan",
                        principalTable: "CityTemplates",
                        principalColumn: "CityId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DemographicProfiles_CityId",
                schema: "finplan",
                table: "DemographicProfiles",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDemographics_SelectedCityId",
                schema: "finplan",
                table: "UserDemographics",
                column: "SelectedCityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactMessages",
                schema: "finplan");

            migrationBuilder.DropTable(
                name: "DemographicProfiles",
                schema: "finplan");

            migrationBuilder.DropTable(
                name: "SurveyResponses",
                schema: "finplan");

            migrationBuilder.DropTable(
                name: "User",
                schema: "finplan");

            migrationBuilder.DropTable(
                name: "UserDemographics",
                schema: "finplan");

            migrationBuilder.DropTable(
                name: "UserRegistration",
                schema: "finplan");

            migrationBuilder.DropTable(
                name: "CityTemplates",
                schema: "finplan");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "finplan",
                table: "FinPlan");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                schema: "finplan",
                table: "FinPlan");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "finplan",
                table: "FinPlan");
        }
    }
}
