using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Dashboard
{
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DashboardModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // User-specific sections
        public IList<Movies.Models.Movie> Watchlist { get; set; } = new List<Movies.Models.Movie>();
        public IList<Movies.Models.Movie> PlannedWatchlist { get; set; } = new List<Movies.Models.Movie>();
        public IList<Movies.Models.Movie> Recommendations { get; set; } = new List<Movies.Models.Movie>();

        // Admin overview
        public IList<IdentityUser> AllUsers { get; set; } = new List<IdentityUser>();
        public IList<Movies.Models.Movie> AllMovies { get; set; } = new List<Movies.Models.Movie>();
        public IList<Director> AllDirectors { get; set; } = new List<Director>();
        public IList<Genre> AllGenres { get; set; } = new List<Genre>();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            // Load user-specific watchlists
            var watchlistEntries = await _context.Watchlists
                .Where(w => w.UserId == user.Id)
                .Include(w => w.Movie)
                .ToListAsync();

            Watchlist = watchlistEntries
                .Where(w => !w.IsPlanned)
                .Select(w => w.Movie!)
                .ToList();

            PlannedWatchlist = watchlistEntries
                .Where(w => w.IsPlanned)
                .Select(w => w.Movie!)
                .ToList();

            // Recommendations based on watchlist genres
            var watchlistMovieIds = watchlistEntries.Select(w => w.MovieId).ToList();
            var watchlistGenres = await _context.MovieGenres
                .Where(mg => watchlistMovieIds.Contains(mg.MovieId))
                .Select(mg => mg.GenreId)
                .Distinct()
                .ToListAsync();

            Recommendations = await _context.Movies
                .Include(m => m.MovieGenres)
                .Where(m => m.MovieGenres.Any(mg => watchlistGenres.Contains(mg.GenreId)) &&
                            !watchlistMovieIds.Contains(m.MovieId))
                .OrderBy(m => m.Title)
                .ToListAsync();

            // Admin sections
            AllUsers = await _userManager.Users.ToListAsync();
            AllMovies = await _context.Movies
                .Include(m => m.Director)
                .Include(m => m.MovieGenres)
                .ToListAsync();
            AllDirectors = await _context.Directors
                .OrderBy(d => d.Name)
                .ToListAsync();
            AllGenres = await _context.Genres
                .OrderBy(g => g.Name)
                .ToListAsync();

            return Page();
        }
    }
}
