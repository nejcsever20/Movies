using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;
using System.Globalization;

namespace Movies.Pages.Friends
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public List<IdentityUser> AllUsers { get; set; } = new();
        public List<IdentityUser> MyFriends { get; set; } = new();
        public List<IdentityUser> Followers { get; set; } = new();
        public List<IdentityUser> Following { get; set; } = new();
        public List<IdentityUser> BlockedUsers { get; set; } = new();
        public List<Message> Messages { get; set; } = new();

        [BindProperty]
        public string ChatWithUserId { get; set; } = "";

        public int FriendCount => MyFriends.Count;
        public int FollowerCount => Followers.Count;
        public int FollowingCount => Following.Count;
        public int BlockedCount => BlockedUsers.Count;

        public async Task OnGetAsync(string search = "")
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return;

            // Load all friendships where this user is involved (either side)
            var friendships = await _context.Friendships
                .Where(f => f.UserId == currentUser.Id || f.FriendId == currentUser.Id)
                .ToListAsync();

            // Load all users (filtered)
            AllUsers = await _userManager.Users
                .Where(u => u.Id != currentUser.Id &&
                            (string.IsNullOrEmpty(search) ||
                             u.UserName.Contains(search) ||
                             u.Email.Contains(search)))
                .ToListAsync();

            // My friends = accepted (non-pending, non-blocked) friendships initiated by me or by others
            var friendIds = friendships
                .Where(f => !f.IsBlocked && !f.IsPending &&
                            (f.UserId == currentUser.Id || f.FriendId == currentUser.Id))
                .Select(f => f.UserId == currentUser.Id ? f.FriendId : f.UserId)
                .Distinct()
                .ToHashSet();

            MyFriends = AllUsers.Where(u => friendIds.Contains(u.Id)).ToList();

            // Following = users I follow (IsFollower = true)
            var followingIds = friendships
                .Where(f => f.UserId == currentUser.Id && f.IsFollower)
                .Select(f => f.FriendId)
                .ToHashSet();

            Following = AllUsers.Where(u => followingIds.Contains(u.Id)).ToList();

            // Followers = users that follow me
            var followerIds = friendships
                .Where(f => f.FriendId == currentUser.Id && f.IsFollower)
                .Select(f => f.UserId)
                .ToHashSet();

            Followers = AllUsers.Where(u => followerIds.Contains(u.Id)).ToList();

            // Blocked users = users I have blocked
            var blockedIds = friendships
                .Where(f => f.UserId == currentUser.Id && f.IsBlocked)
                .Select(f => f.FriendId)
                .ToHashSet();

            BlockedUsers = AllUsers.Where(u => blockedIds.Contains(u.Id)).ToList();

            // Chat messages if in chat mode
            if (!string.IsNullOrEmpty(ChatWithUserId))
            {
                Messages = await _context.Messages
                    .Where(m =>
                        (m.FromUserId == currentUser.Id && m.ToUserId == ChatWithUserId) ||
                        (m.FromUserId == ChatWithUserId && m.ToUserId == currentUser.Id))
                    .OrderBy(m => m.SentAt)
                    .ToListAsync();
            }
        }

        // --- Relationship Actions ---

        public async Task<IActionResult> OnPostAddFriendAsync(string friendId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || string.IsNullOrEmpty(friendId))
                return RedirectToPage();

            var existing = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserId == currentUser.Id && f.FriendId == friendId);

            if (existing == null)
            {
                _context.Friendships.Add(new Friendship
                {
                    UserId = currentUser.Id,
                    FriendId = friendId,
                    IsBlocked = false,
                    IsFollower = false,
                    IsPending = true
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostFollowUserAsync(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || string.IsNullOrEmpty(userId))
                return RedirectToPage();

            var existing = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserId == currentUser.Id && f.FriendId == userId);

            if (existing == null)
            {
                _context.Friendships.Add(new Friendship
                {
                    UserId = currentUser.Id,
                    FriendId = userId,
                    IsFollower = true
                });
            }
            else
            {
                existing.IsFollower = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUnfollowUserAsync(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || string.IsNullOrEmpty(userId))
                return RedirectToPage();

            var existing = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserId == currentUser.Id && f.FriendId == userId);

            if (existing != null)
            {
                existing.IsFollower = false;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBlockUserAsync(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || string.IsNullOrEmpty(userId))
                return RedirectToPage();

            var existing = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserId == currentUser.Id && f.FriendId == userId);

            if (existing != null)
            {
                existing.IsBlocked = true;
                existing.IsPending = false;
            }
            else
            {
                _context.Friendships.Add(new Friendship
                {
                    UserId = currentUser.Id,
                    FriendId = userId,
                    IsBlocked = true,
                    IsPending = false
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUnblockUserAsync(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || string.IsNullOrEmpty(userId))
                return RedirectToPage();

            var existing = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserId == currentUser.Id && f.FriendId == userId);

            if (existing != null)
            {
                existing.IsBlocked = false;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSendMessageAsync(string toUserId, string content)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || string.IsNullOrEmpty(toUserId) || string.IsNullOrEmpty(content))
                return RedirectToPage();

            _context.Messages.Add(new Message
            {
                FromUserId = currentUser.Id,
                ToUserId = toUserId,
                Content = content,
                SentAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return RedirectToPage(new { ChatWithUserId = toUserId });
        }

        public string FormatUserName(string email)
        {
            if (string.IsNullOrEmpty(email)) return "";
            var namePart = email.Split('@')[0];
            var parts = namePart.Split('.', '-', '_');
            return string.Join(" ", parts.Select(p => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p)));
        }

        public bool IsPending(string userId)
        {
            var currentUser = _userManager.GetUserAsync(User).Result;
            if (currentUser == null) return false;

            return _context.Friendships
                .Any(f => f.UserId == currentUser.Id && f.FriendId == userId && f.IsPending);
        }
    }
}