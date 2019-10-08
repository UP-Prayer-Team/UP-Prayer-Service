using Microsoft.EntityFrameworkCore.Migrations;

namespace UPPrayerService.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Confirmations",
                columns: table => new
                {
                    ID = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Confirmations", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Endorsements",
                columns: table => new
                {
                    ID = table.Column<string>(nullable: false),
                    HomepageURL = table.Column<string>(nullable: true),
                    DonateURL = table.Column<string>(nullable: true),
                    Summary = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Endorsements", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    ID = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    District = table.Column<string>(nullable: true),
                    Year = table.Column<int>(nullable: false),
                    MonthIndex = table.Column<int>(nullable: false),
                    DayIndex = table.Column<int>(nullable: false),
                    SlotIndex = table.Column<int>(nullable: false),
                    IsConfirmed = table.Column<bool>(nullable: false),
                    ConfirmationID = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Reservations_Confirmations_ConfirmationID",
                        column: x => x.ConfirmationID,
                        principalTable: "Confirmations",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ConfirmationID",
                table: "Reservations",
                column: "ConfirmationID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Endorsements");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Confirmations");
        }
    }
}
