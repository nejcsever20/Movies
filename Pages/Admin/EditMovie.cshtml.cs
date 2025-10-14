using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class EditMovieModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public EditMovieModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public Movies.Models.Movie Movie { get; set; } = default!;

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

        public async Task<IActionResult> OnPostAsync(IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var movieInDb = await _context.Movies.FindAsync(Movie.MovieId);
            if (movieInDb == null)
            {
                return NotFound();
            }

            // Update basic fields
            movieInDb.Title = Movie.Title;
            movieInDb.Description = Movie.Description;
            movieInDb.ReleaseDate = Movie.ReleaseDate;
            movieInDb.TrailerUrl = Movie.TrailerUrl;
            movieInDb.Rating = Movie.Rating; // ✅ Added rating update

            // Handle new image upload
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images/movies");
                Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                movieInDb.ImagePath = "/images/movies/" + uniqueFileName;
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/DetailsMovie", new { id = Movie.MovieId });
        }
    }
}
