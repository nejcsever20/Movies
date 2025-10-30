using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Movies.Models
{
    public class MovieReview
    {
        public int MovieReviewId { get; set; }  // Primary key
        public int MovieId { get; set; }
        public string? UserId { get; set; }
        public int Rating { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }

        public Movie? Movie { get; set; }
        public IdentityUser? User { get; set; }

        public ICollection<ReviewReaction> Reactions { get; set; } = new List<ReviewReaction>();
    }
}
