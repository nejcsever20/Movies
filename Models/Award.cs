namespace Movies.Models
{
    public class Award
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;

        // Navigation property to UserAwards
        public virtual ICollection<UserAward> UserAwards { get; set; } = new List<UserAward>();
    }
}
