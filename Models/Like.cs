namespace Movies.Models
{
    public class Like
    {
        public int Id { get; set; }
        public string? UserId { get; set; } = null!;
        public int MovieId { get; set; }
    }
}
