using System;
using Microsoft.EntityFrameworkCore.Migrations;

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
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NeedId = table.Column<int>(type: "INTEGER", nullable: false),
                    NeedTitle = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DonorName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    DonorEmail = table.Column<string>(type: "TEXT", maxLength: 160, nullable: true),
                    PledgedGiftAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PlatformSupportPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StripeSessionId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    StripePaymentIntentId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PaymentStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    OffsiteStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    OffsiteReceiptNote = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ConfirmedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiftOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Needs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 140, nullable: false),
                    ShortSummary = table.Column<string>(type: "TEXT", maxLength: 220, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    GoalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountRaised = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RequesterName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    RequesterEmail = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    PayTo = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    InstitutionName = table.Column<string>(type: "TEXT", maxLength: 180, nullable: false),
                    InstitutionType = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    InstitutionPaymentLink = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    PreferDirectToInstitution = table.Column<bool>(type: "INTEGER", nullable: false),
                    VerificationLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    VerificationNote = table.Column<string>(type: "TEXT", maxLength: 600, nullable: false),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    RejectionReason = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    ReviewedBy = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    ReviewedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SubmissionToken = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Needs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pledges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NeedId = table.Column<int>(type: "INTEGER", nullable: false),
                    DonorName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    DonorEmail = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PlatformFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    AdminNote = table.Column<string>(type: "TEXT", maxLength: 600, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    NeedTitleSnapshot = table.Column<string>(type: "TEXT", maxLength: 140, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pledges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 140, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    VerificationLabel = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Tagline = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                    ShortSummary = table.Column<string>(type: "TEXT", maxLength: 220, nullable: false),
                    CoverImagePath = table.Column<string>(type: "TEXT", maxLength: 260, nullable: true),
                    GalleryImagePaths = table.Column<string>(type: "TEXT", nullable: true),
                    Body = table.Column<string>(type: "TEXT", nullable: false),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PublishedUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NeedPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NeedId = table.Column<int>(type: "INTEGER", nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 400, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NeedId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 140, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: false),
                    PublicName = table.Column<string>(type: "TEXT", maxLength: 120, nullable: true),
                    IsThankYou = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsVisible = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
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
