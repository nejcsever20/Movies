using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Admin
{
    [Authorize(Roles = "Admin")]

    public class EditActorModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public EditActorModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public Actor Actor { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Actor = await _context.Actors.FindAsync(id);
            if (Actor == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var actorInDb = await _context.Actors.FindAsync(Actor.ActorId);
            if (actorInDb == null) return NotFound();

            actorInDb.Name = Actor.Name;
            await _context.SaveChangesAsync();

            return RedirectToPage("/Admin/Actors");
        }
    }
}
