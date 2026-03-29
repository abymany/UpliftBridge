using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UpliftBridge.Migrations
{
    public partial class FixStoryBooleanColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider.Contains("Npgsql"))
            {
                migrationBuilder.Sql(@"
                    ALTER TABLE ""Stories""
                    ALTER COLUMN ""IsPublished"" TYPE boolean
                    USING CASE
                        WHEN ""IsPublished"" = 1 THEN true
                        ELSE false
                    END;
                ");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider.Contains("Npgsql"))
            {
                migrationBuilder.Sql(@"
                    ALTER TABLE ""Stories""
                    ALTER COLUMN ""IsPublished"" TYPE integer
                    USING CASE
                        WHEN ""IsPublished"" = true THEN 1
                        ELSE 0
                    END;
                ");
            }
        }
    }
}