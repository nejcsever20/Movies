using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Movies.Models
{
    public class MovieComment
    {
        public int MovieCommentId { get; set; }

        [Required, StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        // Relationships
        public int MovieId { get; set; }
        public Movie Movie { get; set; } = default!;

        public string UserId { get; set; } = string.Empty;
        public IdentityUser User { get; set; } = default!;

        // NEW: for replies
        public int? ParentCommentId { get; set; }
        [ForeignKey("ParentCommentId")]
        public MovieComment? ParentComment { get; set; }

        public List<MovieComment> Replies { get; set; } = new();

        public List<CommentLike> Likes { get; set; } = new();
    }
}
