using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Movies.Data;
using Movies.Models;
using OfficeOpenXml; // For Excel reading
using System.ComponentModel.DataAnnotations;

namespace Movies.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DirectorsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 10; // 👈 10 directors per page

        public DirectorsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // List of directors for the current page
        public IList<Director> Directors { get; set; } = new List<Director>();

        [BindProperty]
        public DirectorInputModel NewDirector { get; set; } = new DirectorInputModel();

        [BindProperty]
        public IFormFile? ImportFile { get; set; }

        // Pagination properties
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;
        public int TotalPages { get; set; }

        public class DirectorInputModel
        {
            [Required(ErrorMessage = "Director name is required")]
            [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
            public string Name { get; set; } = string.Empty;
        }

        public async Task OnGetAsync()
        {
            int totalDirectors = await _context.Directors.CountAsync();
            TotalPages = (int)Math.Ceiling(totalDirectors / (double)PageSize);

            Directors = await _context.Directors
                .OrderBy(d => d.Name)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        // Add single director
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var director = new Director { Name = NewDirector.Name };
            _context.Directors.Add(director);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Director '{director.Name}' created successfully!";
            return RedirectToPage(new { PageNumber }); // Stay on current page
        }

        // Import directors from Excel
        public async Task<IActionResult> OnPostImportAsync()
        {
            if (ImportFile == null || ImportFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a valid Excel file.";
                await OnGetAsync();
                return Page();
            }

            using var stream = ImportFile.OpenReadStream();
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                TempData["ErrorMessage"] = "Excel file is empty.";
                await OnGetAsync();
                return Page();
            }

            var rowCount = worksheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++)
            {
                string? name = worksheet.Cells[row, 1].Text.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    if (!await _context.Directors.AnyAsync(d => d.Name == name))
                    {
                        _context.Directors.Add(new Director { Name = name });
                    }
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Directors imported successfully!";
            return RedirectToPage();
        }
    }
}
