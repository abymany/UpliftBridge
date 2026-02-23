using System.ComponentModel.DataAnnotations;

namespace UpliftBridge.Models
{
    public class NeedUpdateCreateViewModel
    {
        [Required]
        public int NeedId { get; set; }

        [Required, MaxLength(140)]
        public string Title { get; set; } = "";

        [Required, MaxLength(3000)]
        public string Message { get; set; } = "";

        [MaxLength(120)]
        public string PublicName { get; set; } = ""; // optional, e.g. "A grateful student"
    }
}
