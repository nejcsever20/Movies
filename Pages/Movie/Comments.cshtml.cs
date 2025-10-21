using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Movie
{
    public class CommentsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CommentsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public MovieComment NewComment { get; set; } = new MovieComment();

        public Movies.Models.Movie? Movie { get; set; }
        public List<MovieComment> Comments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Movie = await _context.Movies.FindAsync(id);
            if (Movie == null) return NotFound();

            Comments = await _context.MovieComments
                .Where(c => c.MovieId == id)
                .OrderByDescending(c => c.CreatedAt)
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

            NewComment.MovieId = id;
            NewComment.UserId = user.Id;
            NewComment.CreatedAt = DateTime.UtcNow;

            _context.MovieComments.Add(NewComment);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id });
        }
    }
}
