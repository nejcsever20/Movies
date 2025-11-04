using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Movie
{
    public class CommentsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CommentsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty] public MovieComment NewComment { get; set; } = new();
        [BindProperty] public MovieComment NewReply { get; set; } = new();

        public Movies.Models.Movie Movie { get; set; } = default!;
        public List<MovieComment> Comments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Movie = await _context.Movies.FindAsync(id);
            if (Movie == null) return NotFound();

            // Load top-level comments
            Comments = await _context.MovieComments
                .Include(c => c.User)
                .Include(c => c.Likes)
                .Where(c => c.MovieId == id && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            // Load all replies recursively
            foreach (var comment in Comments)
            {
                await LoadReplies(comment);
            }

            return Page();
        }

        private async Task LoadReplies(MovieComment parent)
        {
            parent.Replies = await _context.MovieComments
                .Include(r => r.User)
                .Include(r => r.Likes)
                .Where(r => r.ParentCommentId == parent.MovieCommentId)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            foreach (var reply in parent.Replies)
            {
                await LoadReplies(reply); // recursive load
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (string.IsNullOrWhiteSpace(NewComment.Content))
            {
                ModelState.AddModelError("NewComment.Content", "Comment cannot be empty.");
                await OnGetAsync(id);
                return Page();
            }

            NewComment.MovieId = id;
            NewComment.UserId = user.Id;
            NewComment.CreatedAt = DateTime.UtcNow;

            _context.MovieComments.Add(NewComment);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostReplyAsync(int parentId, int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (string.IsNullOrWhiteSpace(NewReply.Content))
            {
                ModelState.AddModelError("NewReply.Content", "Reply cannot be empty.");
                await OnGetAsync(id);
                return Page();
            }

            var reply = new MovieComment
            {
                Content = NewReply.Content,
                CreatedAt = DateTime.UtcNow,
                MovieId = id,
                UserId = user.Id,
                ParentCommentId = parentId
            };

            _context.MovieComments.Add(reply);
            await _context.SaveChangesAsync();

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostLikeCommentAsync(int commentId, bool isLike)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var existing = await _context.CommentLikes.FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == user.Id);

            if (existing != null)
            {
                if (existing.IsLike == isLike)
                    _context.CommentLikes.Remove(existing);
                else
                    existing.IsLike = isLike;
            }
            else
            {
                _context.CommentLikes.Add(new CommentLike
                {
                    CommentId = commentId,
                    UserId = user.Id,
                    IsLike = isLike,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            var movieId = (await _context.MovieComments.FindAsync(commentId))?.MovieId;
            return RedirectToPage(new { id = movieId });
        }

        public int GetLikesCount(MovieComment comment) => comment.Likes.Count(l => l.IsLike);
        public int GetDislikesCount(MovieComment comment) => comment.Likes.Count(l => !l.IsLike);
    }
}
