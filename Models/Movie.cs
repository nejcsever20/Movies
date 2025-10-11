using System;
using System.Collections.Generic;
using System.IO;

namespace Movies.Models
{
    public class Movie
    {
        public int MovieId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int? RuntimeMinutes { get; set; }
        public int? DirectorId { get; set; }
        public string? PosterUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        public Director Director { get; set; }
        public ICollection<MovieGenre> MovieGenres { get; set; }
        public ICollection<MovieActor> MovieActors { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Rating> Ratings { get; set; }
        public ICollection<Watchlist> Watchlists { get; set; }
        public ICollection<Photo> Photos { get; set; }
    }
}
