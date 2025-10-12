using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Movies.Models
{
    public class Friendship
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; } = string.Empty;      // current user
        public string? FriendId { get; set; } = string.Empty;    // the friend user
        public bool IsBlocked { get; set; } = false;
        public bool IsFollower { get; set; } = false;
        public bool IsPending { get; set; } = true;
        public bool IsAccepted { get; set; } = false;


        public IdentityUser? User { get; set; }
        public IdentityUser? Friend { get; set; }
    }
}
