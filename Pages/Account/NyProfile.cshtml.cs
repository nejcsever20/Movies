using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Account
{
    public class MyProfileModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public MyProfileModel(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IdentityUser CurrentUser { get; set; } = default!;
        public UserProgress? Progress { get; set; }
        public List<Movies.Models.Movie> Watchlist { get; set; } = new();
        public List<Movies.Models.Movie> LikedMovies { get; set; } = new();
        public List<Movies.Models.Movie> WatchedMovies { get; set; } = new();
        public List<Award> Awards { get; set; } = new();

        public async Task OnGetAsync()
        {
            CurrentUser = await _userManager.GetUserAsync(User) ?? throw new Exception("User not found");

            Progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == CurrentUser.Id);

            Watchlist = await _context.Watchlists
                .Where(w => w.UserId == CurrentUser.Id)
                .Include(w => w.Movie)
                .Select(w => w.Movie!)
                .ToListAsync();

            LikedMovies = await _context.MovieLikes
                .Where(l => l.UserId == CurrentUser.Id)
                .Include(l => l.Movie)
                .Select(l => l.Movie!)
                .ToListAsync();

            WatchedMovies = await _context.UserWatchedMovies
                .Where(w => w.UserId == CurrentUser.Id)
                .Include(w => w.Movie)
                .OrderByDescending(w => w.WatchedOn)
                .Select(w => w.Movie!)
                .ToListAsync();

            Awards = await _context.UserAwards
                .Where(a => a.UserId == CurrentUser.Id)
                .Include(a => a.Award)
                .Select(a => a.Award!)
                .ToListAsync();
        }
    }
}
