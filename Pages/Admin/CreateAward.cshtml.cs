using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Movies.Data;
using Movies.Models;

namespace Movies.Pages.Admin
{
    public class CreateAwardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CreateAwardModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public Award Award { get; set; } = new Award();

        [BindProperty]
        public IFormFile? IconFile { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Handle file upload
            if (IconFile != null && IconFile.Length > 0)
            {
                var fileName = Path.GetFileName(IconFile.FileName);
                var savePath = Path.Combine(_environment.WebRootPath, "images/awards", fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await IconFile.CopyToAsync(stream);
                }

                Award.IconUrl = "/images/awards/" + fileName;
            }

            _context.Awards.Add(Award);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Award created successfully!";
            return RedirectToPage("/Awards/AllAwards");
        }
    }
}
