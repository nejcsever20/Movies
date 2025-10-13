using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Movies.Data;
using Movies.Models;
using OfficeOpenXml;

namespace Movies.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ImportActorsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ImportActorsModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [BindProperty]
        public IFormFile? ExcelFile { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ExcelFile == null || ExcelFile.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select an Excel file to upload.");
                return Page();
            }

            using (var stream = new MemoryStream())
            {
                await ExcelFile.CopyToAsync(stream);
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.First();
                var rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++) // Start at 2 assuming row 1 = header
                {
                    string name = worksheet.Cells[row, 1].Text.Trim();
                    string birthDateStr = worksheet.Cells[row, 2].Text.Trim();
                    string imagePath = worksheet.Cells[row, 3].Text.Trim();

                    if (string.IsNullOrEmpty(name))
                        continue; // skip empty names

                    DateTime? birthDate = null;
                    if (DateTime.TryParse(birthDateStr, out DateTime parsedDate))
                    {
                        birthDate = parsedDate;
                    }

                    var actor = new Actor
                    {
                        Name = name,
                        BirthDate = birthDate,
                        ImagePath = string.IsNullOrWhiteSpace(imagePath) ? null : imagePath,
                        MovieActors = new List<MovieActor>()
                    };

                    _context.Actors.Add(actor);
                }

                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Actors imported successfully!";
            return RedirectToPage("/Admin/CreateActor");
        }
    }
}
