using Movies.Models;

public class MovieLike
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public int MovieId { get; set; }

    public Movie? Movie { get; set; }
}
