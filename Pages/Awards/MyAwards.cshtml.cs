using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Awards
{
    public class MyAwardsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MyAwardsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Award> UserAwards { get; set; } = new();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            // Load user's awards
            UserAwards = await _context.UserAwards
                .Where(a => a.UserId == user.Id)
                .Include(a => a.Award)
                .Select(a => a.Award!)
                .ToListAsync();

            // Special Owner award for specific email
            if (user.Email == "nejc.severmihelic@scv.si")
            {
                var ownerAward = await _context.Awards.FirstOrDefaultAsync(a => a.Name == "Owner");
                if (ownerAward != null && !UserAwards.Any(a => a.Id == ownerAward.Id))
                {
                    UserAwards.Add(ownerAward);

                    if (!await _context.UserAwards.AnyAsync(ua => ua.UserId == user.Id && ua.AwardId == ownerAward.Id))
                    {
                        _context.UserAwards.Add(new UserAward
                        {
                            UserId = user.Id,
                            AwardId = ownerAward.Id
                        });
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
