using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;
using System.Globalization;

namespace Movies.Pages.Friends
{
    public class RequestsModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public RequestsModel(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public List<FriendRequestItem> PendingRequests { get; set; } = new();

        public class FriendRequestItem
        {
            public int Id { get; set; }
            public string SenderId { get; set; } = "";
            public string SenderName { get; set; } = "";
            public DateTime SentAt { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToPage("/Account/Login");

            var pending = await _context.Friendships
                .Where(f => f.FriendId == currentUser.Id && f.IsPending)
                .Include(f => f.User)
                .ToListAsync();

            PendingRequests = pending.Select(f => new FriendRequestItem
            {
                Id = f.Id,
                SenderId = f.UserId,
                SenderName = FormatUserName(f.User.UserName),
                SentAt = DateTime.UtcNow
            }).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAcceptAsync(int requestId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToPage();

            var friendship = await _context.Friendships.FindAsync(requestId);
            if (friendship != null && friendship.FriendId == currentUser.Id)
            {
                friendship.IsPending = false;
                friendship.IsAccepted = true;

                // Add reciprocal friendship (optional)
                _context.Friendships.Add(new Friendship
                {
                    UserId = currentUser.Id,
                    FriendId = friendship.UserId,
                    IsPending = false,
                    IsAccepted = true
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeclineAsync(int requestId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return RedirectToPage();

            var friendship = await _context.Friendships.FindAsync(requestId);
            if (friendship != null && friendship.FriendId == currentUser.Id)
            {
                _context.Friendships.Remove(friendship);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public string FormatUserName(string email)
        {
            if (string.IsNullOrEmpty(email)) return "";
            var namePart = email.Split('@')[0];
            var parts = namePart.Split('.', '-', '_');
            return string.Join(" ", parts.Select(p => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p)));
        }
    }
}
