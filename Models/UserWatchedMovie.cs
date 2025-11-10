namespace Movies.Models
{
    public class UserWatchedMovie
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int MovieId { get; set; }
        public DateTime WatchedOn { get; set; } = DateTime.Now;

        public Movie? Movie { get; set; }

    }
}
