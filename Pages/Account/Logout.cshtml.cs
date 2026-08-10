using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MusicPlayer.Pages.Account
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnPostAsync()
        {
            // Xóa tất cả dữ liệu session
            HttpContext.Session.Clear();
            Console.WriteLine("Session cleared on logout");

            // Đăng xuất người dùng khỏi hệ thống xác thực
            await HttpContext.SignOutAsync();
            Console.WriteLine("User signed out");

            // Chuyển hướng về trang chủ hoặc trang đăng nhập
            
            return RedirectToPage("/Index");
        }
        public IActionResult OnGet()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "🎉 Đăng xuất thành công. Hẹn gặp lại!";
            return RedirectToPage("/Index");
        }
    }
}
