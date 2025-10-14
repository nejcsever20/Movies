using System;
using Microsoft.AspNetCore.Identity;

namespace Movies.Models
{
    public class Watchlist
    {
        public int WatchlistId { get; set; }
        public string? UserId { get; set; } = string.Empty;
        public int MovieId { get; set; }
        public DateTime AddedAt { get; set; }

        public Movie? Movie { get; set; }
        public IdentityUser User { get; set; }

        public bool IsPlanned { get; set; } = false;

    }
}
