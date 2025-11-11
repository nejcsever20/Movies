using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Movies.Models
{
    public class UserAward
    {
        public int Id { get; set; }

        // Identity User ID
        public string UserId { get; set; } = string.Empty;

        // Award ID
        public int AwardId { get; set; }

        // When user earned this award
        public DateTime DateEarned { get; set; } = DateTime.UtcNow;

        // Navigation - Identity user
        [ForeignKey(nameof(UserId))]
        public virtual IdentityUser? User { get; set; }

        // Navigation - Award object
        [ForeignKey(nameof(AwardId))]
        public virtual Award? Award { get; set; }
    }
}
