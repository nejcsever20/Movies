using System.Collections.Generic;

namespace Movies.Models
{
    public class Genre
    {
        public int GenreId { get; set; }
        public string? Name { get; set; }

        public ICollection<MovieGenre> MovieGenres { get; set; }
    }
}
