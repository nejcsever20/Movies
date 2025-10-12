using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;
using System.Globalization;

namespace Movies.Pages.Friends
{
    public class ChatModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ChatModel(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IdentityUser? FriendUser { get; set; }
        public List<Message> Messages { get; set; } = new();
        public bool IsFollowing { get; set; } = false;

        [BindProperty]
        public string NewMessage { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(string friendId)
        {
            if (string.IsNullOrEmpty(friendId))
                return RedirectToPage("/Friends/Index");

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToPage("/Friends/Index");

            FriendUser = await _userManager.FindByIdAsync(friendId);
            if (FriendUser == null)
                return RedirectToPage("/Friends/Index");

            // Load messages between current user and friend
            Messages = await _context.Messages
                .Where(m =>
                    (m.FromUserId == currentUser.Id && m.ToUserId == friendId) ||
                    (m.FromUserId == friendId && m.ToUserId == currentUser.Id))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            // Check if current user is following friend
            var relationship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserId == currentUser.Id && f.FriendId == friendId);

            IsFollowing = relationship != null && relationship.IsFollower;

            return Page();
        }

        public async Task<IActionResult> OnPostSendMessageAsync(string friendId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || string.IsNullOrEmpty(friendId) || string.IsNullOrWhiteSpace(NewMessage))
                return RedirectToPage(new { friendId });

            // Save message
            _context.Messages.Add(new Message
            {
                FromUserId = currentUser.Id,
                ToUserId = friendId,
                Content = NewMessage.Trim(),
                SentAt = DateTime.UtcNow
            });

            // Add notification for the recipient
            _context.Notifications.Add(new Notification
            {
                UserId = friendId,
                Type = "message",
                Content = $"{FormatUserName(currentUser.UserName ?? currentUser.Email ?? "Someone")} sent you a message."
            });

            await _context.SaveChangesAsync();

            return RedirectToPage(new { friendId });
        }

        public async Task<IActionResult> OnPostToggleFollowAsync(string friendId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || string.IsNullOrEmpty(friendId))
                return RedirectToPage(new { friendId });

            var relationship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserId == currentUser.Id && f.FriendId == friendId);

            bool nowFollowing;

            if (relationship != null)
            {
                relationship.IsFollower = !relationship.IsFollower;
                nowFollowing = relationship.IsFollower;
            }
            else
            {
                _context.Friendships.Add(new Friendship
                {
                    UserId = currentUser.Id,
                    FriendId = friendId,
                    IsFollower = true
                });
                nowFollowing = true;
            }

            // If they just followed, create a notification
            if (nowFollowing)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = friendId,
                    Type = "follow",
                    Content = $"{FormatUserName(currentUser.UserName ?? currentUser.Email ?? "Someone")} started following you."
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToPage(new { friendId });
        }

        public string FormatUserName(string? emailOrUserName)
        {
            if (string.IsNullOrEmpty(emailOrUserName)) return "User";
            var namePart = emailOrUserName.Split('@')[0];
            var parts = namePart.Split('.', '-', '_');
            return string.Join(" ", parts.Select(p => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p)));
        }
    }
}
