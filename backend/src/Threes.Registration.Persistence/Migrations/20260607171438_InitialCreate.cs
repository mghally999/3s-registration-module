using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Threes.Registration.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Governorates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Governorates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ProcessedOnUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Registrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    EmailNormalized = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    GovernorateId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cities_Governorates_GovernorateId",
                        column: x => x.GovernorateId,
                        principalTable: "Governorates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GovernorateId = table.Column<int>(type: "int", nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BuildingNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FlatNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Addresses_Governorates_GovernorateId",
                        column: x => x.GovernorateId,
                        principalTable: "Governorates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Addresses_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Governorates",
                columns: new[] { "Id", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, true, "Cairo" },
                    { 2, true, "Giza" },
                    { 3, true, "Alexandria" },
                    { 4, true, "Dakahlia" },
                    { 5, true, "Sharqia" },
                    { 6, true, "Qalyubia" },
                    { 7, true, "Port Said" },
                    { 8, true, "Aswan" }
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "GovernorateId", "IsActive", "Name" },
                values: new object[,]
                {
                    { 101, 1, true, "Nasr City" },
                    { 102, 1, true, "Maadi" },
                    { 103, 1, true, "Heliopolis" },
                    { 104, 1, true, "Zamalek" },
                    { 105, 1, true, "New Cairo" },
                    { 201, 2, true, "Dokki" },
                    { 202, 2, true, "Mohandessin" },
                    { 203, 2, true, "6th of October" },
                    { 204, 2, true, "Haram" },
                    { 205, 2, true, "Sheikh Zayed" },
                    { 301, 3, true, "Smouha" },
                    { 302, 3, true, "Miami" },
                    { 303, 3, true, "Stanley" },
                    { 304, 3, true, "Sidi Gaber" },
                    { 401, 4, true, "Mansoura" },
                    { 402, 4, true, "Mit Ghamr" },
                    { 403, 4, true, "Talkha" },
                    { 501, 5, true, "Zagazig" },
                    { 502, 5, true, "Belbeis" },
                    { 503, 5, true, "10th of Ramadan" },
                    { 601, 6, true, "Banha" },
                    { 602, 6, true, "Shubra El Kheima" },
                    { 603, 6, true, "Qalyub" },
                    { 701, 7, true, "Port Fouad" },
                    { 702, 7, true, "El Manakh" },
                    { 801, 8, true, "Aswan" },
                    { 802, 8, true, "Kom Ombo" },
                    { 803, 8, true, "Edfu" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_CityId",
                table: "Addresses",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_GovernorateId",
                table: "Addresses",
                column: "GovernorateId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_RegistrationId",
                table: "Addresses",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_GovernorateId",
                table: "Cities",
                column: "GovernorateId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc",
                table: "OutboxMessages",
                column: "ProcessedOnUtc");

            migrationBuilder.CreateIndex(
                name: "UX_Registrations_EmailNormalized",
                table: "Registrations",
                column: "EmailNormalized",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Registrations_MobileNumber",
                table: "Registrations",
                column: "MobileNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Registrations");

            migrationBuilder.DropTable(
                name: "Governorates");
        }
    }
}
