using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Movies.Pages.Watchlist
{
    public class RecommendationsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public RecommendationsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Movies.Models.Movie> Recommendations { get; set; } = new List<Movies.Models.Movie>();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            // ✅ Get the user's watchlist movie IDs
            var watchlistMovieIds = await _context.Watchlists
                .Where(w => w.UserId == user.Id)
                .Select(w => w.MovieId)
                .ToListAsync();

            if (!watchlistMovieIds.Any())
            {
                Recommendations = new List<Movies.Models.Movie>();
                return Page();
            }

            // ✅ Get genres from movies in the user's watchlist
            var watchlistGenres = await _context.MovieGenres
                .Where(mg => watchlistMovieIds.Contains(mg.MovieId))
                .Select(mg => mg.GenreId)
                .Distinct()
                .ToListAsync();

            if (!watchlistGenres.Any())
            {
                Recommendations = new List<Movies.Models.Movie>();
                return Page();
            }

            // ✅ Fetch movies that share genres but are not already in watchlist
            Recommendations = await _context.Movies
                .Include(m => m.MovieGenres)
                    .ThenInclude(mg => mg.Genre)
                .Where(m => m.MovieGenres.Any(mg => watchlistGenres.Contains(mg.GenreId))
                            && !watchlistMovieIds.Contains(m.MovieId))
                .OrderBy(m => m.Title)
                .ToListAsync();

            return Page();
        }
    }
}
