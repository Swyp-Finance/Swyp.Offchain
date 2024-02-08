using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Swyp.Sync.Migrations
{
    /// <inheritdoc />
    public partial class TeddyByAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TeddyByAddress",
                schema: "public",
                columns: table => new
                {
                    Address = table.Column<string>(type: "text", nullable: false),
                    Slot = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeddyByAddress", x => new { x.Address, x.Slot });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TeddyByAddress",
                schema: "public");
        }
    }
}
