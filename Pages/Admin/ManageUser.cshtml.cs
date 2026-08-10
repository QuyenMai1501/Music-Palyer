using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicPlayer.Data;
using MusicPlayer.Models;
using MusicPlayer.Helpers;
using Microsoft.EntityFrameworkCore;

namespace MusicPlayer.Pages.Admin
{
    public class ManageModel(AppDbContext context) : PageModel
    {
        private readonly AppDbContext _context = context;
        public IList<User> Users { get; set; } = new List<User>();

        [BindProperty(SupportsGet = true)]
        public int PageIndex {get; set;} = 1;
        public int PageSize {get; set;} = 6;
        public int TotalPages {get; set;} 


        public async Task<IActionResult> OnGetAsync()
        { 
            if (HttpContext.Session.GetUserRole() != "Admin")
            {
                return RedirectToPage("/Account/Login");
            }

            var query = _context.Users
                .Where(u => u.Role != "Admin")
                .OrderBy(u => u.Username);

            int totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            

            Users = await query
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostPromoteToAdminAsync(int userId)
        {
            if (HttpContext.Session.GetUserRole() != "Admin")
            {
                return RedirectToPage("/Account/Login");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user != null && user.Role != "Admin")
            {
                user.Role = "Admin";
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Người dùng đã được nâng quyền lên Admin.";
            }
            else
            {
                TempData["ErrorMessage"] = "Người dùng không tồn tại hoặc đã có quyền Admin";
            }
            return RedirectToPage("/Admin/ManageUser");
        }

        public async Task<IActionResult> OnPostLockUserAsync(int userId)
        {
            if (HttpContext.Session.GetUserRole() != "Admin")
            {
                return RedirectToPage("/Account/Login");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user != null && !user.IsLocked)
            {
                user.IsLocked = true;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tài khoản người dùng đã bị khóa";
            }
            else
            {
                TempData["ErrorMessage"] = "Người dùng không tồn tại hoặc tài khoản đã bị khóa.";
            }
            return RedirectToPage("/Admin/ManageUser");
        }

        public async Task<IActionResult> OnPostUnlockUserAsync(int userId)
        {
            if (HttpContext.Session.GetUserRole() != "Admin")
            {
                return RedirectToPage("/Account/Login");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user != null && user.IsLocked)
            {
                user.IsLocked = false;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tài khoản người dùng đã được mở khóa khóa";
            }
            else
            {
                TempData["ErrorMessage"] = "Người dùng không tồn tại hoặc tài khoản đã mở khóa.";
            }
            return RedirectToPage("/Admin/ManageUser");
        }
    }
}