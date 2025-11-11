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

            // XP/Level
            Progress = await _context.UserProgresses
                .FirstOrDefaultAsync(p => p.UserId == CurrentUser.Id);

            // Watchlist
            Watchlist = await _context.Watchlists
                .Where(w => w.UserId == CurrentUser.Id)
                .Include(w => w.Movie)
                .Select(w => w.Movie!)
                .ToListAsync();

            // Liked Movies
            LikedMovies = await _context.MovieLikes
                .Where(l => l.UserId == CurrentUser.Id)
                .Include(l => l.Movie)
                .Select(l => l.Movie!)
                .ToListAsync();

            // Watched Movies
            WatchedMovies = await _context.UserWatchedMovies
                .Where(w => w.UserId == CurrentUser.Id)
                .Include(w => w.Movie)
                .OrderByDescending(w => w.WatchedOn)
                .Select(w => w.Movie!)
                .ToListAsync();

            // Awards already earned
            Awards = await _context.UserAwards
                .Where(a => a.UserId == CurrentUser.Id)
                .Include(a => a.Award)
                .Select(a => a.Award!)
                .ToListAsync();

            // Special Owner Award
            if (CurrentUser.Email == "nejc.severmihelic@scv.si")
            {
                var ownerAward = await _context.Awards.FirstOrDefaultAsync(a => a.Name == "OWNER");
                if (ownerAward != null && !Awards.Any(a => a.Id == ownerAward.Id))
                {
                    Awards.Add(ownerAward);

                    // Add to database if not already assigned
                    if (!await _context.UserAwards.AnyAsync(ua => ua.UserId == CurrentUser.Id && ua.AwardId == ownerAward.Id))
                    {
                        _context.UserAwards.Add(new UserAward
                        {
                            UserId = CurrentUser.Id,
                            AwardId = ownerAward.Id
                        });
                        await _context.SaveChangesAsync();
                    }
                }

                var ownerAward2 = await _context.Awards.FirstOrDefaultAsync(a => a.Name == "Owner");
                if (ownerAward2 != null && !Awards.Any(a => a.Id == ownerAward2.Id))
                {
                    Awards.Add(ownerAward2);

                    // Add to database if not already assigned
                    if (!await _context.UserAwards.AnyAsync(ua => ua.UserId == CurrentUser.Id && ua.AwardId == ownerAward2.Id))
                    {
                        _context.UserAwards.Add(new UserAward
                        {
                            UserId = CurrentUser.Id,
                            AwardId = ownerAward2.Id
                        });
                        await _context.SaveChangesAsync();
                    }
                }


                var developerAward = await _context.Awards.FirstOrDefaultAsync(a => a.Name == "Developer");
                if (developerAward != null && !Awards.Any(a => a.Id == developerAward.Id))
                {
                    Awards.Add(developerAward);

                    // Add to database if not already assigned
                    if (!await _context.UserAwards.AnyAsync(ua => ua.UserId == CurrentUser.Id && ua.AwardId == developerAward.Id))
                    {
                        _context.UserAwards.Add(new UserAward
                        {
                            UserId = CurrentUser.Id,
                            AwardId = developerAward.Id
                        });
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
