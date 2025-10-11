using System;
using Microsoft.AspNetCore.Identity;

namespace Movies.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int MovieId { get; set; }
        public string? UserId { get; set; }
        public string? Content { get; set; }
        public DateTime CreatedAt { get; set; }

        public Movie Movie { get; set; }
        public IdentityUser User { get; set; }
    }
}
