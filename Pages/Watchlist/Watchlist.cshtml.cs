using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Watchlist
{
    public class WatchlistModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public WatchlistModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Movies.Models.Movie> Movies { get; set; } = new List<Movies.Models.Movie>();

        // Load the user's watchlist
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            Movies = await _context.Watchlists
                .Where(w => w.UserId == user.Id)
                .Include(w => w.Movie)
                .Select(w => w.Movie!)
                .OrderBy(m => m.Title)
                .ToListAsync();

            return Page();
        }

        // Remove movie from watchlist
        public async Task<IActionResult> OnPostRemoveAsync(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var entry = await _context.Watchlists
                .FirstOrDefaultAsync(w => w.UserId == user.Id && w.MovieId == movieId);

            if (entry != null)
            {
                _context.Watchlists.Remove(entry);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        // Mark movie as planned
        public async Task<IActionResult> OnPostPlanAsync(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Challenge();

            var entry = await _context.Watchlists
                .FirstOrDefaultAsync(w => w.UserId == user.Id && w.MovieId == movieId);

            if (entry != null)
            {
                entry.IsPlanned = true; // mark as planned
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Movie marked as planned!";
            return RedirectToPage();
        }
    }
}
