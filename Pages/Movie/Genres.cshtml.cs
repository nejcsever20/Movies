using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Movie
{
    public class GenresModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public GenresModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Genre> Genres { get; set; } = new List<Genre>();
        public bool IsAdmin { get; set; } = false;

        public async Task OnGetAsync()
        {
            // Check if current user is admin
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                IsAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            }

            // Load all genres with their movies efficiently
            Genres = await _context.Genres
                .Include(g => g.MovieGenres)
                    .ThenInclude(mg => mg.Movie)
                .OrderBy(g => g.Name)
                .ToListAsync();

            // Remove duplicate movies per genre if necessary
            foreach (var genre in Genres)
            {
                genre.MovieGenres = genre.MovieGenres
                    .GroupBy(mg => mg.MovieId)
                    .Select(g => g.First())
                    .ToList();
            }
        }
    }
}
