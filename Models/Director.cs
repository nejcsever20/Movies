using System;
using System.Collections.Generic;

namespace Movies.Models
{
    public class Director
    {
        public int DirectorId { get; set; }
        public string? Name { get; set; }
        public string? Bio { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? ProfileImageUrl { get; set; }

        public ICollection<Movie> Movies { get; set; }
    }
}
