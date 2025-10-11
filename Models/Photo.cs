using System;
using Microsoft.AspNetCore.Identity;

namespace Movies.Models
{
    public class Photo
    {
        public int PhotoId { get; set; }
        public int? MovieId { get; set; }
        public string? UserId { get; set; }
        public string? Url { get; set; }
        public string? Caption { get; set; }
        public DateTime UploadedAt { get; set; }

        public Movie Movie { get; set; }
        public IdentityUser User { get; set; }
    }
}
