using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PIQI.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrimaryUnitMart",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContentSetMnemonic = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    CodeSystemMnemonic = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CodeValue = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    UOMText = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrimaryUnitMart", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RangeSetMart",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContentSetMnemonic = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    CodeSystemMnemonic = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CodeValue = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    UOMText = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MinValue = table.Column<double>(type: "REAL", nullable: true),
                    MaxValue = table.Column<double>(type: "REAL", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RangeSetMart", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TextMart",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContentSetMnemonic = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TextValue = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextMart", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PrimaryUnitMart",
                columns: new[] { "Id", "CodeSystemMnemonic", "CodeValue", "ContentSetMnemonic", "UOMText" },
                values: new object[] { 1, "REGEN_LOINC", "100374-8", "PC_UOM", "[GPL'U]" });

            migrationBuilder.InsertData(
                table: "RangeSetMart",
                columns: new[] { "Id", "CodeSystemMnemonic", "CodeValue", "ContentSetMnemonic", "MaxValue", "MinValue", "UOMText" },
                values: new object[] { 1, "REGEN_LOINC", "2075-0", "PC_UOM_VALUE_RANGE", 400.0, 0.0, "mmol/L" });

            migrationBuilder.InsertData(
                table: "TextMart",
                columns: new[] { "Id", "ContentSetMnemonic", "TextValue" },
                values: new object[] { 1, "UCUM", "mg/L" });

            migrationBuilder.CreateIndex(
                name: "IX_PrimaryUnitMart_ContentSetMnemonic_CodeSystemMnemonic_CodeValue",
                table: "PrimaryUnitMart",
                columns: new[] { "ContentSetMnemonic", "CodeSystemMnemonic", "CodeValue" });

            migrationBuilder.CreateIndex(
                name: "IX_RangeSetMart_ContentSetMnemonic_CodeSystemMnemonic_CodeValue",
                table: "RangeSetMart",
                columns: new[] { "ContentSetMnemonic", "CodeSystemMnemonic", "CodeValue" });

            migrationBuilder.CreateIndex(
                name: "IX_TextMart_ContentSetMnemonic",
                table: "TextMart",
                column: "ContentSetMnemonic");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrimaryUnitMart");

            migrationBuilder.DropTable(
                name: "RangeSetMart");

            migrationBuilder.DropTable(
                name: "TextMart");
        }
    }
}
