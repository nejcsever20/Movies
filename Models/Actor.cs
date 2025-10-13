using System.ComponentModel.DataAnnotations;

namespace Movies.Models
{
    public class Actor
    {
        [Key]
        public int ActorId { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        // Make ImagePath optional
        public string? ImagePath { get; set; }

        // Initialize collections to avoid required errors
        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}
