using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Admin
{
    public class EditGenreModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public EditGenreModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public Genre Genre { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Genre = await _context.Genres.FindAsync(id);
            if (Genre == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var genreInDb = await _context.Genres.FindAsync(Genre.GenreId);
            if (genreInDb == null) return NotFound();

            genreInDb.Name = Genre.Name;
            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Genres");
        }
    }
}
