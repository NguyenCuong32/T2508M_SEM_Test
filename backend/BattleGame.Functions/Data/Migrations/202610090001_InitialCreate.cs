using BattleGame.Functions.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BattleGame.Functions.Data.Migrations;

[DbContext(typeof(BattleGameDbContext))]
[Migration("202610090001_InitialCreate")]
public sealed class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Asset",
            columns: table => new
            {
                AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AssetName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                LevelRequire = table.Column<int>(type: "int", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Asset", x => x.AssetId);
                table.CheckConstraint("CK_Asset_LevelRequire", "[LevelRequire] >= 0");
            });

        migrationBuilder.CreateTable(
            name: "Player",
            columns: table => new
            {
                PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                PlayerName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                FullName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                Age = table.Column<int>(type: "int", nullable: false),
                Level = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                Email = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Player", x => x.PlayerId);
                table.CheckConstraint("CK_Player_Age", "[Age] BETWEEN 1 AND 120");
                table.CheckConstraint("CK_Player_Level", "[Level] >= 1");
            });

        migrationBuilder.CreateTable(
            name: "PlayerAsset",
            columns: table => new
            {
                PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PlayerAsset", x => new { x.PlayerId, x.AssetId });
                table.ForeignKey(
                    name: "FK_PlayerAsset_Asset_AssetId",
                    column: x => x.AssetId,
                    principalTable: "Asset",
                    principalColumn: "AssetId",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_PlayerAsset_Player_PlayerId",
                    column: x => x.PlayerId,
                    principalTable: "Player",
                    principalColumn: "PlayerId",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Asset_AssetName",
            table: "Asset",
            column: "AssetName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Player_Email",
            table: "Player",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Player_PlayerName",
            table: "Player",
            column: "PlayerName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_PlayerAsset_AssetId",
            table: "PlayerAsset",
            column: "AssetId");

        SeedDemoData(migrationBuilder);
    }

    private static void SeedDemoData(MigrationBuilder migrationBuilder)
    {
        var player1 = new Guid("11111111-1111-1111-1111-111111111111");
        var player2 = new Guid("22222222-2222-2222-2222-222222222222");
        var player3 = new Guid("33333333-3333-3333-3333-333333333333");
        var hero1 = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var hero2 = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        migrationBuilder.InsertData(
            table: "Player",
            columns: ["PlayerId", "PlayerName", "FullName", "Age", "Level", "Email"],
            values: new object[,]
            {
                { player1, "Player 1", "Demo Player One", 20, 10, "player1@example.com" },
                { player2, "Player 2", "Demo Player Two", 19, 3, "player2@example.com" },
                { player3, "Player 3", "Demo Player Three", 23, 10, "player3@example.com" }
            });

        migrationBuilder.InsertData(
            table: "Asset",
            columns: ["AssetId", "AssetName", "LevelRequire"],
            values: new object[,]
            {
                { hero1, "Hero 1", 1 },
                { hero2, "Hero 2", 1 }
            });

        migrationBuilder.InsertData(
            table: "PlayerAsset",
            columns: ["PlayerId", "AssetId"],
            values: new object[,]
            {
                { player1, hero1 },
                { player2, hero2 },
                { player3, hero1 }
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PlayerAsset");
        migrationBuilder.DropTable(name: "Asset");
        migrationBuilder.DropTable(name: "Player");
    }
}
