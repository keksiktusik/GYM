using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApp.Migrations
{
    /// <inheritdoc />
    public partial class AddTypZajec : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TypyZajec",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nazwa = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PoziomTrudnosci = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Trener = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CzasTrwania = table.Column<int>(type: "int", nullable: false),
                    Cena = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Kategoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DataUtworzenia = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CzyAktywne = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypyZajec", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TypyZajec");
        }
    }
}
