using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;

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

            // Get the list of movie IDs in the user's watchlist
            var watchlistMovieIds = await _context.Watchlists
                .Where(w => w.UserId == user.Id)
                .Select(w => w.MovieId)
                .ToListAsync();

            // Get recommended movies: any movie that shares the same genres as the user's watchlist movies
            var watchlistGenres = await _context.MovieGenres
                .Where(mg => watchlistMovieIds.Contains(mg.MovieId))
                .Select(mg => mg.GenreId)
                .Distinct()
                .ToListAsync();

            // Fetch movies that share these genres but are not already in the watchlist
            Recommendations = await _context.Movies
                .Include(m => m.MovieGenres)
                .Where(m => m.MovieGenres.Any(mg => watchlistGenres.Contains(mg.GenreId)) &&
                            !watchlistMovieIds.Contains(m.MovieId))
                .OrderBy(m => m.Title)
                .ToListAsync();

            return Page();
        }
    }
}
