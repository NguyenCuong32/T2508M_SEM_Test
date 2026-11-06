using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SunriseHotel.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Room",
                columns: table => new
                {
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    RoomNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RoomType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    PricePerNight = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxGuests = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Room", x => x.RoomId);
                    table.CheckConstraint("CK_Room_MaxGuests", "[MaxGuests] > 0");
                    table.CheckConstraint("CK_Room_PricePerNight", "[PricePerNight] >= 0");
                    table.CheckConstraint("CK_Room_RoomType", "[RoomType] IN ('Single','Double','Suite')");
                });

            migrationBuilder.CreateTable(
                name: "Booking",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    GuestName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CheckIn = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckOut = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Booking", x => x.BookingId);
                    table.CheckConstraint("CK_Booking_Dates", "[CheckOut] > [CheckIn]");
                    table.CheckConstraint("CK_Booking_Status", "[Status] IN ('Confirmed','Cancelled')");
                    table.CheckConstraint("CK_Booking_TotalPrice", "[TotalPrice] >= 0");
                    table.ForeignKey(
                        name: "FK_Booking_Room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Room",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Room",
                columns: new[] { "RoomId", "MaxGuests", "PricePerNight", "RoomNumber", "RoomType" },
                values: new object[,]
                {
                    { 1, 1, 600000m, "101", "Single" },
                    { 2, 1, 600000m, "102", "Single" },
                    { 3, 2, 950000m, "201", "Double" },
                    { 4, 2, 950000m, "202", "Double" },
                    { 5, 4, 2200000m, "301", "Suite" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Booking_Phone",
                table: "Booking",
                column: "Phone");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_RoomId_CheckIn_CheckOut",
                table: "Booking",
                columns: new[] { "RoomId", "CheckIn", "CheckOut" });

            migrationBuilder.CreateIndex(
                name: "IX_Room_RoomNumber",
                table: "Room",
                column: "RoomNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Booking");

            migrationBuilder.DropTable(
                name: "Room");
        }
    }
}
