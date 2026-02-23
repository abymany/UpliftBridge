using System;
using System.Linq;
using UpliftBridge.Models;

namespace UpliftBridge.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            // If there are already Needs, don't seed again
            if (context.Needs.Any())
            {
                return;
            }

            context.Needs.AddRange(
                new Need
                {
                    Title = "Laptops for village school",
                    Description = "10 basic laptops so students in Punjab can learn computer skills.",
                    Category = NeedCategory.Education,
                    Location = "Hope Valley Public School, Punjab, India",
                    GoalAmount = 15000m,
                    AmountRaised = 6000m,
                    RequesterName = "School principal",
                    RequesterEmail = "principal@hopevalley.edu",
                    VerificationLevel = VerificationLevel.OrganizationVerified,
                    VerificationNote = "Verified with school letter and phone call.",
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new Need
                {
                    Title = "Running shoes for young athlete",
                    Description = "A regional runner raising funds for proper shoes and training kit.",
                    Category = NeedCategory.Sports,
                    Location = "Detroit, USA",
                    GoalAmount = 1000m,
                    AmountRaised = 700m,
                    RequesterName = "Coach Ravi",
                    RequesterEmail = "coach@example.com",
                    VerificationLevel = VerificationLevel.CommunityVerified,
                    VerificationNote = "Confirmed by local coach.",
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-7)
                },
                new Need
                {
                    Title = "Groceries for single mother",
                    Description = "One-time support for monthly groceries and school snacks for her kids.",
                    Category = NeedCategory.Family,
                    Location = "Nairobi, Kenya",
                    GoalAmount = 500m,
                    AmountRaised = 275m,
                    RequesterName = "Grace",
                    RequesterEmail = "grace@example.com",
                    VerificationLevel = VerificationLevel.BasicContactVerified,
                    VerificationNote = "Contact and story checked by local NGO.",
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                }
            );

            context.SaveChanges();
        }
    }
}
