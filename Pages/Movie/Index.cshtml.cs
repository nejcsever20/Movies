using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Movie
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private IList<Models.Movie> moviesList = default!;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Movies.Models.Movie> MoviesList { get => moviesList; set => moviesList = value; }
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        public async Task OnGetAsync(int pageNumber = 1)
        {
            PageNumber = pageNumber;
            int pageSize = 40;

            var query = _context.Movies
                .Include(m => m.Director)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(m => m.Title.Contains(SearchTerm));
            }

            query = query.OrderByDescending(m => m.ReleaseDate);

            int totalMovies = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalMovies / (double)pageSize);

            MoviesList = await query
                .Skip((PageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
