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

        // GET: Movies list with pagination
        public async Task<IActionResult> OnGetAsync(int pageNumber = 1)
        {
            PageNumber = pageNumber;
            int pageSize = 40;

            var query = _context.Movies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
                query = query.Where(m => m.Title.Contains(SearchTerm));

            query = query.OrderByDescending(m => m.ReleaseDate);

            int totalMovies = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalMovies / (double)pageSize);

            MoviesList = await query.Skip((PageNumber - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToListAsync();

            return Page();
        }

        // Add to Watchlist
        public async Task<IActionResult> OnPostAddToWatchlistAsync(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!await _context.Watchlists.AnyAsync(w => w.UserId == user.Id && w.MovieId == movieId))
            {
                _context.Watchlists.Add(new Movies.Models.Watchlist { UserId = user.Id, MovieId = movieId });
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Movie added to your watchlist!";
            return RedirectToPage();
        }

        // Like Movie
        public async Task<IActionResult> OnPostLikeMovieAsync(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!await _context.MovieLikes.AnyAsync(l => l.UserId == user.Id && l.MovieId == movieId))
            {
                _context.MovieLikes.Add(new MovieLike { UserId = user.Id, MovieId = movieId });
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        // Remove Like
        public async Task<IActionResult> OnPostRemoveLikeAsync(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var like = await _context.MovieLikes.FirstOrDefaultAsync(l => l.UserId == user.Id && l.MovieId == movieId);
            if (like != null)
            {
                _context.MovieLikes.Remove(like);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        // Get likes count for a movie
        public async Task<int> GetLikesCountAsync(int movieId)
        {
            return await _context.MovieLikes.CountAsync(l => l.MovieId == movieId);
        }

        public async Task<bool> UserLikedMovieAsync(string userId, int movieId)
        {
            return await _context.MovieLikes.AnyAsync(l => l.UserId == userId && l.MovieId == movieId);
        }

        // Mark a movie as watched
        public async Task<IActionResult> OnPostMarkWatchedAsync(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!await _context.UserWatchedMovies.AnyAsync(w => w.UserId == user.Id && w.MovieId == movieId))
            {
                // Add watched record
                _context.UserWatchedMovies.Add(new UserWatchedMovie
                {
                    UserId = user.Id,
                    MovieId = movieId,
                    WatchedOn = DateTime.Now
                });

                // Add XP based on movie runtime
                var movie = await _context.Movies.FindAsync(movieId);
                if (movie != null)
                {
                    var progress = await _context.UserProgresses.FirstOrDefaultAsync(u => u.UserId == user.Id)
                        ?? new UserProgress { UserId = user.Id, XP = 0, Level = 1 };

                    progress.XP += movie.RuntimeMinutes;
                    progress.Level = (progress.XP / 100) + 1;

                    if (progress.Id == 0)
                        _context.UserProgresses.Add(progress);

                    // Check for awards
                    await CheckAwards(user.Id, progress.XP);
                }

                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Movie marked as watched!";
            return RedirectToPage();
        }

        // Check if user has watched a specific movie
        public async Task<bool> UserWatchedMovieAsync(string userId, int movieId)
        {
            return await _context.UserWatchedMovies.AnyAsync(w => w.UserId == userId && w.MovieId == movieId);
        }

        // Award logic
        private async Task CheckAwards(string userId, int xp)
        {
            var userAwards = await _context.UserAwards
                .Where(a => a.UserId == userId)
                .Select(a => a.AwardId)
                .ToListAsync();

            var awardsToGive = new List<Award>();

            if (xp >= 500 && !userAwards.Contains(2)) awardsToGive.Add(await _context.Awards.FindAsync(2));
            if (xp >= 1000 && !userAwards.Contains(3)) awardsToGive.Add(await _context.Awards.FindAsync(3));

            foreach (var award in awardsToGive)
            {
                if (award != null)
                    _context.UserAwards.Add(new UserAward { UserId = userId, AwardId = award.Id });
            }

            await _context.SaveChangesAsync();
        }
    }
}
