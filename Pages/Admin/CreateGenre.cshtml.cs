using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Movies.Data;
using Movies.Models;
using System.ComponentModel.DataAnnotations;

namespace Movies.Pages.Admin
{
    [Authorize(Roles = "Admin")] // Only admins can access
    public class CreateGenreModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateGenreModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Bind property to capture form input
        [BindProperty]
        public GenreInputModel NewGenre { get; set; } = new();

        // Input model to allow validation
        public class GenreInputModel
        {
            [Required(ErrorMessage = "Genre name is required")]
            [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
            public string Name { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Save genre to database
            var genre = new Genre
            {
                Name = NewGenre.Name
            };

            _context.Genres.Add(genre);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Genre '{genre.Name}' created successfully!";
            return RedirectToPage("/Admin/CreateGenre");
        }
    }
}
