using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Admin
{
    public class MoviesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public MoviesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Movies.Models.Movie> Movies { get; set; } = new List<Movies.Models.Movie>();

        public async Task OnGetAsync()
        {
            Movies = await _context.Movies.OrderBy(m => m.Title).ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteMovieAsync(int movieId)
        {
            var movie = await _context.Movies.FindAsync(movieId);
            if (movie != null)
            {
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
