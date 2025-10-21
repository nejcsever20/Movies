using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Movies.Models
{
    public class ReviewReaction
    {
        [Key]
        public int ReactionId { get; set; }

        [Required]
        public int ReviewId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public bool IsLike { get; set; }

        [ForeignKey("ReviewId")]
        public MovieReview? Review { get; set; }
    }
}
