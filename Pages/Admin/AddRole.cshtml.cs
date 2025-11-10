using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[Authorize(Roles = "Admin")]
public class AddRoleModel : PageModel
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private const int PageSize = 10; // 10 roles per page

    public AddRoleModel(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    [BindProperty]
    public string RoleName { get; set; }

    public IList<IdentityRole> Roles { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }

    public async Task OnGetAsync(int pageNumber = 1)
    {
        CurrentPage = pageNumber;

        var allRoles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
        TotalPages = (int)Math.Ceiling(allRoles.Count / (double)PageSize);

        Roles = allRoles
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }

    public async Task<IActionResult> OnPostAddRoleAsync(int pageNumber = 1)
    {
        if (!string.IsNullOrWhiteSpace(RoleName))
        {
            if (!await _roleManager.RoleExistsAsync(RoleName))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(RoleName));
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Error creating role.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Role already exists.");
            }
        }

        return RedirectToPage(new { pageNumber });
    }

    public async Task<IActionResult> OnPostDeleteRoleAsync(string roleId, int pageNumber = 1)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role != null)
        {
            await _roleManager.DeleteAsync(role);
        }

        return RedirectToPage(new { pageNumber });
    }
}
