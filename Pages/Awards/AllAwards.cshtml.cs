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

        // IDs of awards the user has unlocked
        public List<int> UserAwardIds { get; set; } = new List<int>();

        // Current user progress
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
                }
            }
        }
    }
}
