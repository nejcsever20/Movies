using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Movie
{
    public class ReviewsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReviewsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public MovieReview NewReview { get; set; } = new MovieReview();

        public Movies.Models.Movie? Movie { get; set; }
        public List<MovieReview> Reviews { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Movie = await _context.Movies.FindAsync(id);
            if (Movie == null) return NotFound();

            Reviews = await _context.MovieReviews
                .Include(r => r.Reactions)
                .Where(r => r.MovieId == id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                await OnGetAsync(id);
                return Page();
            }

            NewReview.MovieId = id;
            NewReview.UserId = user.Id;
            NewReview.CreatedAt = DateTime.UtcNow;

            _context.MovieReviews.Add(NewReview);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id });
        }

        // Handle like/dislike
        public async Task<IActionResult> OnPostReactAsync(int reviewId, bool isLike)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var existing = await _context.ReviewReactions
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.UserId == user.Id);

            if (existing == null)
            {
                _context.ReviewReactions.Add(new ReviewReaction
                {
                    ReviewId = reviewId,
                    UserId = user.Id,
                    IsLike = isLike
                });
            }
            else
            {
                // Toggle or update
                if (existing.IsLike == isLike)
                    _context.ReviewReactions.Remove(existing);
                else
                    existing.IsLike = isLike;
            }

            await _context.SaveChangesAsync();
            var review = await _context.MovieReviews.FindAsync(reviewId);
            return RedirectToPage(new { id = review?.MovieId });
        }
    }
}
