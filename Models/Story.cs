using System;
using System.ComponentModel.DataAnnotations;

namespace UpliftBridge.Models
{
    public class Story
    {
        public int Id { get; set; }

        [Required, MaxLength(140)]
        public string Title { get; set; } = "";

        [MaxLength(60)]
        public string Category { get; set; } = "Education";

        [MaxLength(120)]
        public string Location { get; set; } = "—";

        [MaxLength(120)]
        public string VerificationLabel { get; set; } = "Partner-verified";

        [MaxLength(160)]
        public string Tagline { get; set; } = "";

        [MaxLength(220)]
        public string ShortSummary { get; set; } = "";

        // Example: "~/images/stories/story-education.jpg"
        [MaxLength(260)]
        public string? CoverImagePath { get; set; }

        // Semicolon-separated: "~/images/stories/a.jpg;~/images/stories/b.jpg"
        public string? GalleryImagePaths { get; set; }

        [Required]
        public string Body { get; set; } = "";

        public bool IsPublished { get; set; } = false;

        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? PublishedUtc { get; set; }
    }
}
