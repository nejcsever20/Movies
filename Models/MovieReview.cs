using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Movies.Models
{
    public class MovieReview
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        public int MovieId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required, StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        [Range(1, 10)]
        public int Rating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("MovieId")]
        public Movie? Movie { get; set; }

        public ICollection<ReviewReaction> Reactions { get; set; } = new List<ReviewReaction>();
    }
}
