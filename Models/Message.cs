using System.ComponentModel.DataAnnotations;

namespace Movies.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        public string FromUserId { get; set; } = string.Empty;
        public string ToUserId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
