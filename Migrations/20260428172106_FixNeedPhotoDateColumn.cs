using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UpliftBridge.Migrations
{
    public partial class FixNeedPhotoDateColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
            ALTER TABLE "NeedPhotos"
            ALTER COLUMN "CreatedAtUtc" TYPE timestamp with time zone
            USING NULLIF("CreatedAtUtc"::text, '')::timestamp with time zone;
            """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
            ALTER TABLE "NeedPhotos"
            ALTER COLUMN "CreatedAtUtc" TYPE text
            USING "CreatedAtUtc"::text;
            """);
        }
    }
}