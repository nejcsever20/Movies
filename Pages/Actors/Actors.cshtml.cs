using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ActorsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ActorsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Actor> Actors { get; set; } = new List<Actor>();

        public async Task OnGetAsync()
        {
            var actors = await _context.Actors
                .Include(a => a.MovieActors)
                .ThenInclude(ma => ma.Movie)
                .OrderBy(a => a.Name)
                .ToListAsync();

            // Remove duplicates by Name (case-insensitive)
            Actors = actors
                .GroupBy(a => a.Name.Trim().ToLower())
                .Select(g => g.First())
                .ToList();
        }
    }
}
