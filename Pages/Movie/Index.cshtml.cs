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

        // Publicly accessible movie list
        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
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

            return Page();
        }

        // Add to Watchlist
        public async Task<IActionResult> OnPostAddToWatchlistAsync(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var existing = await _context.Watchlists
                .FirstOrDefaultAsync(w => w.UserId == user.Id && w.MovieId == movieId);

            if (existing == null)
            {
                _context.Watchlists.Add(new Movies.Models.Watchlist
                {
                    UserId = user.Id,
                    MovieId = movieId
                });
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Movie added to your watchlist!";
            return RedirectToPage();
        }

        // Like Movie
        public async Task<IActionResult> OnPostLikeMovieAsync(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var existingLike = await _context.MovieLikes
                .FirstOrDefaultAsync(l => l.UserId == user.Id && l.MovieId == movieId);

            if (existingLike == null)
            {
                _context.MovieLikes.Add(new MovieLike
                {
                    UserId = user.Id,
                    MovieId = movieId
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        // Remove Like
        public async Task<IActionResult> OnPostRemoveLikeAsync(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var existingLike = await _context.MovieLikes
                .FirstOrDefaultAsync(l => l.UserId == user.Id && l.MovieId == movieId);

            if (existingLike != null)
            {
                _context.MovieLikes.Remove(existingLike);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<int> GetLikesCountAsync(int movieId)
        {
            return await _context.MovieLikes.CountAsync(l => l.MovieId == movieId);
        }

        public async Task<bool> UserLikedMovieAsync(string userId, int movieId)
        {
            return await _context.MovieLikes.AnyAsync(l => l.UserId == userId && l.MovieId == movieId);
        }
    }
}
