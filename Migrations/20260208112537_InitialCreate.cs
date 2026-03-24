using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickRegister.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "app_state",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_state", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "bedrijven",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    klantId = table.Column<int>(type: "INTEGER", nullable: false),
                    BedrijfNaam = table.Column<string>(type: "TEXT", nullable: false),
                    StraatNaam = table.Column<string>(type: "TEXT", nullable: true),
                    AdresNummer = table.Column<string>(type: "TEXT", nullable: true),
                    Postcode = table.Column<string>(type: "TEXT", nullable: true),
                    Stad = table.Column<string>(type: "TEXT", nullable: true),
                    Land = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bedrijven", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "interventies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Machine = table.Column<string>(type: "TEXT", nullable: false),
                    TotaleLooptijd = table.Column<int>(type: "INTEGER", nullable: false),
                    IdRecentsteCall = table.Column<int>(type: "INTEGER", nullable: false),
                    KlantId = table.Column<int>(type: "INTEGER", nullable: false),
                    BedrijfNaam = table.Column<string>(type: "TEXT", nullable: false),
                    StraatNaam = table.Column<string>(type: "TEXT", nullable: true),
                    AdresNummer = table.Column<string>(type: "TEXT", nullable: true),
                    Postcode = table.Column<string>(type: "TEXT", nullable: true),
                    Stad = table.Column<string>(type: "TEXT", nullable: true),
                    Land = table.Column<string>(type: "TEXT", nullable: true),
                    Afgerond = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interventies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "medewerkers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Naam = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medewerkers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "interventie_call",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    InterventieId = table.Column<int>(type: "INTEGER", nullable: false),
                    MedewerkerId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContactpersoonNaam = table.Column<string>(type: "TEXT", nullable: false),
                    ContactpersoonEmail = table.Column<string>(type: "TEXT", nullable: true),
                    ContactpersoonTelefoonNummer = table.Column<string>(type: "TEXT", nullable: true),
                    InterneNotities = table.Column<string>(type: "TEXT", nullable: true),
                    ExterneNotities = table.Column<string>(type: "TEXT", nullable: true),
                    StartCall = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EindCall = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_interventie_call", x => x.Id);
                    table.ForeignKey(
                        name: "FK_interventie_call_interventies_InterventieId",
                        column: x => x.InterventieId,
                        principalTable: "interventies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_interventie_call_medewerkers_MedewerkerId",
                        column: x => x.MedewerkerId,
                        principalTable: "medewerkers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_interventie_call_InterventieId",
                table: "interventie_call",
                column: "InterventieId");

            migrationBuilder.CreateIndex(
                name: "IX_interventie_call_MedewerkerId",
                table: "interventie_call",
                column: "MedewerkerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_state");

            migrationBuilder.DropTable(
                name: "bedrijven");

            migrationBuilder.DropTable(
                name: "interventie_call");

            migrationBuilder.DropTable(
                name: "interventies");

            migrationBuilder.DropTable(
                name: "medewerkers");
        }
    }
}
