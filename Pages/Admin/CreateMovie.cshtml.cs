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
        private readonly IWebHostEnvironment _environment;

        public CreateMovieModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public Movies.Models.Movie NewMovie { get; set; } = new Movies.Models.Movie();

        [BindProperty]
        public int SelectedGenreId { get; set; }

        [BindProperty]
        public IFormFile? ImageFile { get; set; }  // <-- Added for image upload

        public IList<Director> Directors { get; set; } = new List<Director>();
        public IList<Genre> Genres { get; set; } = new List<Genre>();

        public async Task OnGetAsync()
        {
            Directors = await _context.Directors.OrderBy(d => d.Name).ToListAsync();
            Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Directors = await _context.Directors.OrderBy(d => d.Name).ToListAsync();
                Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
                return Page();
            }

            // Handle Image Upload
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "movies");
                Directory.CreateDirectory(uploadsFolder); // Ensure folder exists
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                NewMovie.ImagePath = "/images/movies/" + uniqueFileName; // Save relative path in DB
            }

            _context.Movies.Add(NewMovie);
            await _context.SaveChangesAsync();

            // Create MovieGenre linking table entry
            if (SelectedGenreId > 0)
            {
                var movieGenre = new MovieGenre
                {
                    MovieId = NewMovie.MovieId,
                    GenreId = SelectedGenreId
                };
                _context.MovieGenres.Add(movieGenre);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"Movie '{NewMovie.Title}' created successfully!";
            return RedirectToPage("/Admin/Movies");
        }
    }
}
