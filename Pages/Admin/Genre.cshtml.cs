using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Admin
{
    public class GenresModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public GenresModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Genre> Genres { get; set; } = new List<Genre>();

        public async Task OnGetAsync()
        {
            Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteGenreAsync(int genreId)
        {
            var genre = await _context.Genres.FindAsync(genreId);
            if (genre != null)
            {
                _context.Genres.Remove(genre);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
