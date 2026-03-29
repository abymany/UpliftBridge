using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UpliftBridge.Migrations
{
    /// <inheritdoc />
    public partial class AddInstitutionFullAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstitutionFullAddress",
                table: "Needs",
                type: "TEXT",
                maxLength: 400,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstitutionFullAddress",
                table: "Needs");
        }
    }
}
