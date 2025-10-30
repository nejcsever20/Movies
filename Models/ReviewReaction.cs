using Microsoft.AspNetCore.Identity;

namespace Movies.Models
{
    public class ReviewReaction
    {
        public int ReviewReactionId { get; set; }  // Primary key
        public int MovieReviewId { get; set; }
        public string? UserId { get; set; }
        public bool IsLike { get; set; }

        public MovieReview MovieReview { get; set; }
        public IdentityUser User { get; set; }
    }
}
