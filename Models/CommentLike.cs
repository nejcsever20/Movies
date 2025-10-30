using Microsoft.AspNetCore.Identity;

namespace Movies.Models
{
    public class CommentLike
    {
        public int CommentLikeId { get; set; }

        // FK to MovieComment
        public int CommentId { get; set; }
        public MovieComment? Comment { get; set; }

        // FK to User
        public string UserId { get; set; } = string.Empty;
        public IdentityUser? User { get; set; }

        // True = Like, False = Dislike
        public bool IsLike { get; set; }

        // Optional timestamp
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
