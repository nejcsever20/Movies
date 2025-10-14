using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Admin.Actors
{
    [Authorize(Roles = "Admin")]
    public class ActorsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ActorsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Actor> Actors { get; set; } = new List<Actor>();

        public async Task OnGetAsync()
        {
            Actors = await _context.Actors.OrderBy(a => a.Name).ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteActorAsync(int actorId)
        {
            var actor = await _context.Actors.FindAsync(actorId);
            if (actor != null)
            {
                _context.Actors.Remove(actor);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}
