using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Movies.Pages.Admin
{
    public class UsersModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IList<IdentityUser> Users { get; set; } = new List<IdentityUser>();
        public Dictionary<string, List<string>> UserRoles { get; set; } = new();
        public IList<string> AllRoles { get; set; } = new List<string>();

        // GET: List all users with their roles
        public async Task OnGetAsync()
        {
            Users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();

            // Load roles
            AllRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

            // Map each user to their roles
            foreach (var user in Users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                UserRoles[user.Id] = roles.ToList();
            }
        }

        // POST: Delete user
        public async Task<IActionResult> OnPostDeleteUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Invalid user ID.";
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User '{user.Email}' deleted successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Error deleting user.";
            }

            return RedirectToPage();
        }

        // POST: Assign role to user
        public async Task<IActionResult> OnPostAssignRoleAsync(string userId, string roleName)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
            {
                TempData["ErrorMessage"] = "Invalid input.";
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage();
            }

            // Create role if it doesn’t exist
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // Assign role if not already assigned
            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                var result = await _userManager.AddToRoleAsync(user, roleName);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"Role '{roleName}' assigned to user '{user.Email}'.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Error assigning role.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = $"User '{user.Email}' already has the '{roleName}' role.";
            }

            return RedirectToPage();
        }
    }
}
