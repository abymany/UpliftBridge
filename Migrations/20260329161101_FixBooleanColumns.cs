using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UpliftBridge.Migrations
{
    /// <inheritdoc />
    public partial class FixBooleanColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider.Contains("Npgsql"))
            {
                migrationBuilder.Sql(@"
                    ALTER TABLE ""Needs""
                    ALTER COLUMN ""IsPublished"" TYPE boolean
                    USING CASE
                        WHEN ""IsPublished"" = 1 THEN true
                        ELSE false
                    END;
                ");

                migrationBuilder.Sql(@"
                    ALTER TABLE ""Needs""
                    ALTER COLUMN ""PreferDirectToInstitution"" TYPE boolean
                    USING CASE
                        WHEN ""PreferDirectToInstitution"" = 1 THEN true
                        ELSE false
                    END;
                ");

                migrationBuilder.Sql(@"
                    ALTER TABLE ""NeedUpdates""
                    ALTER COLUMN ""IsVisible"" TYPE boolean
                    USING CASE
                        WHEN ""IsVisible"" = 1 THEN true
                        ELSE false
                    END;
                ");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider.Contains("Npgsql"))
            {
                migrationBuilder.Sql(@"
                    ALTER TABLE ""NeedUpdates""
                    ALTER COLUMN ""IsVisible"" TYPE integer
                    USING CASE
                        WHEN ""IsVisible"" = true THEN 1
                        ELSE 0
                    END;
                ");

                migrationBuilder.Sql(@"
                    ALTER TABLE ""Needs""
                    ALTER COLUMN ""PreferDirectToInstitution"" TYPE integer
                    USING CASE
                        WHEN ""PreferDirectToInstitution"" = true THEN 1
                        ELSE 0
                    END;
                ");

                migrationBuilder.Sql(@"
                    ALTER TABLE ""Needs""
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
