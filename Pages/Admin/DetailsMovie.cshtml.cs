using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DetailsMovieModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsMovieModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Single movie for the details view
        public Movies.Models.Movie Movie { get; set; } = default!;

        // GET: /Admin/DetailsMovie/{id}
        public async Task<IActionResult> OnGetAsync(int id)
        {
            Movie = await _context.Movies
                                  .Include(m => m.Director)
                                  .FirstOrDefaultAsync(m => m.MovieId == id);

            if (Movie == null)
            {
                return NotFound();
            }

            return Page();
        }

        // Optional: Delete handler
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("/Movie/Index");
        }
    }
}
