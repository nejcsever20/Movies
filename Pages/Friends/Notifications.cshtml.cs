using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;
using System.Globalization;

namespace Movies.Pages.Friends
{
    public class NotificationsModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public NotificationsModel(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public List<NotificationItem> Notifications { get; set; } = new();

        public class NotificationItem
        {
            public string Type { get; set; } = "";
            public string Message { get; set; } = "";
            public DateTime CreatedAt { get; set; }
            public string? FromUserId { get; set; }
            public string? FromUserName { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToPage("/Account/Login");

            // 1️⃣ Friend requests (pending)
            var pendingFriends = await _context.Friendships
                .Where(f => f.FriendId == currentUser.Id && f.IsPending)
                .Include(f => f.User) // ✅ include navigation property
                .ToListAsync();

            foreach (var f in pendingFriends)
            {
                if (f.User != null)
                {
                    Notifications.Add(new NotificationItem
                    {
                        Type = "Friend Request",
                        FromUserId = f.User.Id,
                        FromUserName = FormatUserName(f.User.UserName),
                        Message = $"{FormatUserName(f.User.UserName)} sent you a friend request.",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // 2️⃣ New followers
            var newFollowers = await _context.Friendships
                .Where(f => f.FriendId == currentUser.Id && f.IsFollower)
                .Include(f => f.User) // ✅ include navigation property
                .ToListAsync();

            foreach (var f in newFollowers)
            {
                if (f.User != null)
                {
                    Notifications.Add(new NotificationItem
                    {
                        Type = "Follower",
                        FromUserId = f.User.Id,
                        FromUserName = FormatUserName(f.User.UserName),
                        Message = $"{FormatUserName(f.User.UserName)} started following you.",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // 3️⃣ New messages
            var messages = await _context.Messages
                .Where(m => m.ToUserId == currentUser.Id)
                .OrderByDescending(m => m.SentAt)
                .Take(10)
                .ToListAsync();

            foreach (var m in messages)
            {
                var sender = await _userManager.FindByIdAsync(m.FromUserId);
                if (sender != null)
                {
                    Notifications.Add(new NotificationItem
                    {
                        Type = "Message",
                        FromUserId = sender.Id,
                        FromUserName = FormatUserName(sender.UserName),
                        Message = $"{FormatUserName(sender.UserName)} sent you a message.",
                        CreatedAt = m.SentAt
                    });
                }
            }

            // Sort newest first
            Notifications = Notifications.OrderByDescending(n => n.CreatedAt).ToList();

            return Page();
        }

        public string FormatUserName(string? email)
        {
            if (string.IsNullOrEmpty(email)) return "";
            var namePart = email.Split('@')[0];
            var parts = namePart.Split('.', '-', '_');
            return string.Join(" ", parts.Select(p => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p)));
        }
    }
}
