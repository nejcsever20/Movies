using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CreateMovieModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateMovieModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Movies.Models.Movie NewMovie { get; set; } = new Movies.Models.Movie();

        public IList<Director> Directors { get; set; } = new List<Director>();
        public IList<Genre> Genres { get; set; } = new List<Genre>(); // Add this

        public async Task OnGetAsync()
        {
            Directors = await _context.Directors.OrderBy(d => d.Name).ToListAsync();
            Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync(); // Load genres
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            _context.Movies.Add(NewMovie);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Movie/Index");
        }
    }
}
