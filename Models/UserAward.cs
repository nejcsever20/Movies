using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Movies.Models
{
    public class UserAward
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int AwardId { get; set; }
        public DateTime DateEarned { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey(nameof(UserId))]
        public virtual IdentityUser? User { get; set; }

        [ForeignKey(nameof(AwardId))]
        public virtual Award? Award { get; set; }
    }
}
