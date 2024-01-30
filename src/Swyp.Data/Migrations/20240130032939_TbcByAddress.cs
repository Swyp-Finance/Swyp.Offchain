using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Swyp.Data.Migrations
{
    /// <inheritdoc />
    public partial class TbcByAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "TransactionOutputs",
                newName: "TransactionOutputs",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Blocks",
                newName: "Blocks",
                newSchema: "public");

            migrationBuilder.CreateTable(
                name: "TbcByAddress",
                schema: "public",
                columns: table => new
                {
                    Address = table.Column<string>(type: "text", nullable: false),
                    Slot = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Amount_Coin = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Amount_MultiAssetJson = table.Column<JsonElement>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TbcByAddress", x => new { x.Address, x.Slot });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TbcByAddress",
                schema: "public");

            migrationBuilder.RenameTable(
                name: "TransactionOutputs",
                schema: "public",
                newName: "TransactionOutputs");

            migrationBuilder.RenameTable(
                name: "Blocks",
                schema: "public",
                newName: "Blocks");
        }
    }
}
