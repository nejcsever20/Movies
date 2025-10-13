using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Movies.Pages.Movie
{
    public class TrailersModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public TrailersModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Movies.Models.Movie> Movies { get; set; } = new List<Movies.Models.Movie>();

        public async Task OnGetAsync()
        {
            Movies = await _context.Movies
                                   .Include(m => m.Director)
                                   .ToListAsync();
        }
    }
}
