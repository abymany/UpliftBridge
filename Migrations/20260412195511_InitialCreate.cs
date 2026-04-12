using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UpliftBridge.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GiftOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NeedId = table.Column<int>(type: "integer", nullable: false),
                    NeedTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DonorName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    DonorEmail = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    PledgedGiftAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PlatformSupportPaid = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    StripeSessionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StripePaymentIntentId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
                    OffsiteStatus = table.Column<int>(type: "integer", nullable: false),
                    OffsiteReceiptNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfirmedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiftOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Needs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    ShortSummary = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Location = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    GoalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    AmountRaised = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RequesterName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    RequesterEmail = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    IsPhoneVerified = table.Column<bool>(type: "boolean", nullable: false),
                    EmailOtpCode = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: false),
                    EmailOtpExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmailVerifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PayTo = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    InstitutionName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    InstitutionType = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    InstitutionFullAddress = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    InstitutionPaymentLink = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    PreferDirectToInstitution = table.Column<bool>(type: "boolean", nullable: false),
                    VerificationLevel = table.Column<int>(type: "integer", nullable: false),
                    VerificationNote = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    VerifiedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    VerifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsSuspicious = table.Column<bool>(type: "boolean", nullable: false),
                    RiskNotes = table.Column<string>(type: "character varying(800)", maxLength: 800, nullable: false),
                    InternalReviewStatus = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    InternalReviewNotes = table.Column<string>(type: "character varying(1200)", maxLength: 1200, nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    RejectionReason = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    ReviewedBy = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    ReviewedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SubmissionToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Needs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pledges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NeedId = table.Column<int>(type: "integer", nullable: false),
                    DonorName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    DonorEmail = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PlatformFee = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AdminNote = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NeedTitleSnapshot = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pledges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    Category = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Location = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    VerificationLabel = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Tagline = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    ShortSummary = table.Column<string>(type: "character varying(220)", maxLength: 220, nullable: false),
                    CoverImagePath = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: true),
                    GalleryImagePaths = table.Column<string>(type: "text", nullable: true),
                    Body = table.Column<string>(type: "text", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PublishedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NeedPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NeedId = table.Column<int>(type: "integer", nullable: false),
                    Path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NeedPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NeedPhotos_Needs_NeedId",
                        column: x => x.NeedId,
                        principalTable: "Needs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NeedUpdates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NeedId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: false),
                    Message = table.Column<string>(type: "character varying(3000)", maxLength: 3000, nullable: false),
                    PublicName = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    IsThankYou = table.Column<bool>(type: "boolean", nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NeedUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NeedUpdates_Needs_NeedId",
                        column: x => x.NeedId,
                        principalTable: "Needs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NeedPhotos_NeedId",
                table: "NeedPhotos",
                column: "NeedId");

            migrationBuilder.CreateIndex(
                name: "IX_NeedUpdates_NeedId",
                table: "NeedUpdates",
                column: "NeedId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GiftOrders");

            migrationBuilder.DropTable(
                name: "NeedPhotos");

            migrationBuilder.DropTable(
                name: "NeedUpdates");

            migrationBuilder.DropTable(
                name: "Pledges");

            migrationBuilder.DropTable(
                name: "Stories");

            migrationBuilder.DropTable(
                name: "Needs");
        }
    }
}
