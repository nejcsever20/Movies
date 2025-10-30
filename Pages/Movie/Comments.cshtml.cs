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

        public Movies.Models.Movie Movie { get; set; }
        public List<MovieComment> Comments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Movie = await _context.Movies.FindAsync(id);
            if (Movie == null) return NotFound();

            Comments = await _context.MovieComments
                .Include(c => c.User)
                .Include(c => c.Likes)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.User)
                .Include(c => c.Replies)
                    .ThenInclude(r => r.Likes)
                .Where(c => c.MovieId == id && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
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

            var parentComment = await _context.MovieComments.FindAsync(parentId);
            if (parentComment == null) return NotFound();

            var reply = new MovieComment
            {
                Content = NewReply.Content,
                CreatedAt = DateTime.UtcNow,
                MovieId = parentComment.MovieId,
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

            var existing = await _context.CommentLikes
                .FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == user.Id);

            if (existing != null)
            {
                if (existing.IsLike == isLike)
                {
                    _context.CommentLikes.Remove(existing);
                }
                else
                {
                    existing.IsLike = isLike;
                    _context.CommentLikes.Update(existing);
                }
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

        public async Task<string?> GetUserVoteAsync(MovieComment comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            var vote = comment.Likes.FirstOrDefault(l => l.UserId == user.Id);
            return vote == null ? null : (vote.IsLike ? "like" : "dislike");
        }
    }
}
