using System;
using System.Collections.Generic;

namespace Movies.Models
{
    public class Actor
    {
        public int ActorId { get; set; }
        public string? Name { get; set; }
        public string? Bio { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? ProfileImageUrl { get; set; }

        public ICollection<MovieActor> MovieActors { get; set; }
    }
}
