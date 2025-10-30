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
        public MovieReview NewReview { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }  // MovieId

        public Movies.Models.Movie? Movie { get; set; }
        public List<MovieReview> Reviews { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            Movie = await _context.Movies.FindAsync(Id);
            if (Movie == null) return NotFound();

            Reviews = await _context.MovieReviews
                .Include(r=>r.Reactions)
                .Include(r=>r.User)
                .Where(r=>r.MovieId == Id)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            Movie = await _context.Movies.FindAsync(Id);
            if (Movie == null) return NotFound();

            // Bind MovieId manually to ensure correct binding
            NewReview.MovieId = Id;
            NewReview.UserId = user.Id;
            NewReview.CreatedAt = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(NewReview.Content))
            {
                ModelState.AddModelError("NewReview.Content", "Content cannot be empty.");
            }

            if (!ModelState.IsValid)
            {
                Reviews = await _context.MovieReviews
                    .Include(r => r.Reactions)
                    .Where(r => r.MovieId == Id)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();
                return Page();
            }

            _context.MovieReviews.Add(NewReview);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { Id });
        }
    }
}
