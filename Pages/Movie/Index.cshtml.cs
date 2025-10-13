using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Movie
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Movies.Models.Movie> Movies { get; set; } = default!;
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; }

        public async Task OnGetAsync(int pageNumber = 1)
        {
            PageNumber = pageNumber;
            int pageSize = 40;

            var query = _context.Movies
                .Include(m => m.Director)
                .OrderByDescending(m => m.ReleaseDate);

            int totalMovies = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalMovies / (double)pageSize);

            Movies = await query
                .Skip((PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
