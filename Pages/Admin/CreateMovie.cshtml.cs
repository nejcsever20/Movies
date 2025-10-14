using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;
using OfficeOpenXml;

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
        public IFormFile? ImageFile { get; set; }

        [BindProperty]
        public IFormFile? ExcelFile { get; set; }

        public IList<Director> Directors { get; set; } = new List<Director>();
        public IList<Genre> Genres { get; set; } = new List<Genre>();
        public IList<Actor> Actors { get; set; } = new List<Actor>();

        public async Task OnGetAsync()
        {
            Directors = await _context.Directors.OrderBy(d => d.Name).ToListAsync();
            Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
            Actors = await _context.Actors.OrderBy(a => a.Name).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadDropdownsAsync();

            NewMovie.CreatedAt = DateTime.UtcNow;

            // Initialize collections
            NewMovie.MovieActors ??= new List<MovieActor>();
            NewMovie.Reviews ??= new List<Review>();
            NewMovie.Ratings ??= new List<Rating>();
            NewMovie.Watchlists ??= new List<Movies.Models.Watchlist>(); // fully qualified
            NewMovie.Photos ??= new List<Photo>();


            // Handle image upload
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "movies");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                await ImageFile.CopyToAsync(fileStream);

                NewMovie.ImagePath = "/images/movies/" + uniqueFileName;
            }

            if (!ModelState.IsValid)
                return Page();

            _context.Movies.Add(NewMovie);
            await _context.SaveChangesAsync();

            // Link genre
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
            return RedirectToPage("/Movie/Index");
        }

        public async Task<IActionResult> OnPostImportExcelAsync()
        {
            await LoadDropdownsAsync();

            if (ExcelFile == null || ExcelFile.Length == 0)
            {
                ModelState.AddModelError("", "Please select an Excel file.");
                return Page();
            }

            using var stream = new MemoryStream();
            await ExcelFile.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
            {
                ModelState.AddModelError("", "No worksheet found in Excel file.");
                return Page();
            }

            int rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++) // first row = headers
            {
                string title = worksheet.Cells[row, 1].Text.Trim();
                string description = worksheet.Cells[row, 2].Text.Trim();
                string releaseDateStr = worksheet.Cells[row, 3].Text.Trim();
                string runtimeStr = worksheet.Cells[row, 4].Text.Trim();
                string directorName = worksheet.Cells[row, 5].Text.Trim();
                string actorsStr = worksheet.Cells[row, 6].Text.Trim();
                string ratingStr = worksheet.Cells[row, 7].Text.Trim();

                if (string.IsNullOrEmpty(title)) continue;

                var movie = new Movies.Models.Movie
                {
                    Title = title,
                    Description = description,
                    CreatedAt = DateTime.UtcNow
                };

                // Release Date
                if (DateTime.TryParse(releaseDateStr, out var releaseDate))
                    movie.ReleaseDate = releaseDate;

                // Runtime
                if (int.TryParse(runtimeStr, out var runtime))
                    movie.RuntimeMinutes = runtime;

                // Director
                var director = await _context.Directors.FirstOrDefaultAsync(d => d.Name == directorName);
                if (director != null)
                    movie.DirectorId = director.DirectorId;

                // Actors
                movie.MovieActors = new List<MovieActor>();
                foreach (var actorName in actorsStr.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    var actor = await _context.Actors.FirstOrDefaultAsync(a => a.Name == actorName.Trim());
                    if (actor != null)
                        movie.MovieActors.Add(new MovieActor { ActorId = actor.ActorId });
                }

                // Rating (converted to int)
                if (decimal.TryParse(ratingStr, out var rating))
                    movie.Rating = (int)Math.Round(rating);

                _context.Movies.Add(movie);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Movies imported successfully!";
            return RedirectToPage("/Movie/Index");
        }

        private async Task LoadDropdownsAsync()
        {
            Directors = await _context.Directors.OrderBy(d => d.Name).ToListAsync();
            Genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();
            Actors = await _context.Actors.OrderBy(a => a.Name).ToListAsync();
        }
    }
}
