using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Movie
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Movies.Models.Movie> MoviesList { get; set; } = new List<Movies.Models.Movie>();
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public async Task OnGetAsync(int pageNumber = 1)
        {
            PageNumber = pageNumber;
            int pageSize = 40;

            var query = _context.Movies
                .Include(m => m.Director)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(m => m.Title.Contains(SearchTerm));
            }

            query = query.OrderByDescending(m => m.ReleaseDate);

            int totalMovies = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalMovies / (double)pageSize);

            MoviesList = await query
                .Skip((PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAddToWatchlistAsync(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge(); // redirect to login

            var existing = await _context.Watchlists
                .FirstOrDefaultAsync(w => w.UserId == user.Id && w.MovieId == movieId);

            if (existing == null)
            {
                var watchlistEntry = new Movies.Models.Watchlist
                {
                    UserId = user.Id,
                    MovieId = movieId
                };
                _context.Watchlists.Add(watchlistEntry);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Movie added to your watchlist!";
            return RedirectToPage();
        }
    }
}
