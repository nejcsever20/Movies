using Microsoft.AspNetCore.Authorization;
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

        // List of genres with their movies
        public IList<Genre> Genres { get; set; } = default!;
        public bool IsAdmin { get; set; } = false;

        public async Task OnGetAsync()
        {
            // Check if current user is admin
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                IsAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            }

            // Load all genres and include related movies
            Genres = await _context.Genres
                .Include(g => g.MovieGenres)
                    .ThenInclude(mg => mg.Movie)
                .OrderBy(g => g.Name)
                .ToListAsync();
        }
    }
}
