using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Movies.Models
{
    public class MovieComment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public int MovieId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required, StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("MovieId")]
        public Movie? Movie { get; set; }
    }
}
