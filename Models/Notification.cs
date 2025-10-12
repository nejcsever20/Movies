using System.ComponentModel.DataAnnotations;

namespace Movies.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = default!; // Receiver

        [Required]
        public string Type { get; set; } = "message"; // message, friend_request, follow, etc.

        [Required]
        public string Content { get; set; } = default!;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
