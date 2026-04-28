using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UpliftBridge.Migrations
{
    public partial class FixNeedDateColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
            ALTER TABLE "Needs"
            ALTER COLUMN "CreatedAt" TYPE timestamp with time zone
            USING NULLIF("CreatedAt"::text, '')::timestamp with time zone;
            """);

            migrationBuilder.Sql("""
            ALTER TABLE "Needs"
            ALTER COLUMN "ReviewedAtUtc" TYPE timestamp with time zone
            USING NULLIF("ReviewedAtUtc"::text, '')::timestamp with time zone;
            """);

            migrationBuilder.Sql("""
            ALTER TABLE "Needs"
            ALTER COLUMN "VerifiedAtUtc" TYPE timestamp with time zone
            USING NULLIF("VerifiedAtUtc"::text, '')::timestamp with time zone;
            """);

            migrationBuilder.Sql("""
            ALTER TABLE "Needs"
            ALTER COLUMN "EmailOtpExpiresAtUtc" TYPE timestamp with time zone
            USING NULLIF("EmailOtpExpiresAtUtc"::text, '')::timestamp with time zone;
            """);

            migrationBuilder.Sql("""
            ALTER TABLE "Needs"
            ALTER COLUMN "EmailVerifiedAtUtc" TYPE timestamp with time zone
            USING NULLIF("EmailVerifiedAtUtc"::text, '')::timestamp with time zone;
            """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
            ALTER TABLE "Needs"
            ALTER COLUMN "CreatedAt" TYPE text
            USING "CreatedAt"::text;
            """);

            migrationBuilder.Sql("""
            ALTER TABLE "Needs"
            ALTER COLUMN "ReviewedAtUtc" TYPE text
            USING "ReviewedAtUtc"::text;
            """);

            migrationBuilder.Sql("""
            ALTER TABLE "Needs"
            ALTER COLUMN "VerifiedAtUtc" TYPE text
            USING "VerifiedAtUtc"::text;
            """);

            migrationBuilder.Sql("""
            ALTER TABLE "Needs"
            ALTER COLUMN "EmailOtpExpiresAtUtc" TYPE text
            USING "EmailOtpExpiresAtUtc"::text;
            """);

            migrationBuilder.Sql("""
            ALTER TABLE "Needs"
            ALTER COLUMN "EmailVerifiedAtUtc" TYPE text
            USING "EmailVerifiedAtUtc"::text;
            """);
        }
    }
}