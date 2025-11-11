using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Awards
{
    public class AllAwardsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AllAwardsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Award> AllAwards { get; set; } = new List<Award>();

        // list of unlocked awards IDs
        public List<int> UserAwardIds { get; set; } = new List<int>();

        // user's progress (Level, XP)
        public UserProgress? Progress { get; set; }

        public async Task OnGetAsync()
        {
            AllAwards = await _context.Awards.ToListAsync();

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    UserAwardIds = await _context.UserAwards
                        .Where(ua => ua.UserId == user.Id)
                        .Select(ua => ua.AwardId)
                        .ToListAsync();

                    Progress = await _context.UserProgresses
                        .FirstOrDefaultAsync(p => p.UserId == user.Id);

                    // Auto-unlock awards based on the user's progress
                    if(Progress != null)
                    {
                        var newlyUnlocked = AllAwards
                            .Where(a => !UserAwardIds.Contains(a.Id) && Progress.Level >= a.RequirementLevel)
                            .ToList();

                        foreach (var award in newlyUnlocked)
                        {
                            _context.UserAwards.Add(new UserAward
                            {
                                UserId = user.Id,
                                AwardId = award.Id,
                                DateEarned = DateTime.Now
                            });
                        }

                        if (newlyUnlocked.Count > 0)
                        {
                            await _context.SaveChangesAsync();
                            UserAwardIds.AddRange(newlyUnlocked.Select(a => a.Id));
                        }
                    }
                }
            }
        }
    }
}
