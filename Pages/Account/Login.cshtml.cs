using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicPlayer.Data;
using MusicPlayer.Helpers;

namespace MusicPlayer.Pages.Account
{
    public class LoginModel(AppDbContext db) : PageModel
    {
        private readonly AppDbContext _db = db;

        [BindProperty]
        [Required(ErrorMessage = "Tên người dùng không được để trống.")]
        public required string Username { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Mật khẩu không được để trống.")]

        public required string Password { get; set; }

        public IActionResult OnPost()
        {
            // Nếu đã có lỗi để trống, trả về trang
            if (ModelState.ErrorCount > 0 && ModelState.Any(m => m.Key == "Username" || m.Key == "Password"))
            {
                return Page();
            }

            // Kiểm tra thông tin đăng nhập
            var user = _db.Users.FirstOrDefault(u => u.Username == Username);
            if (user == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return Page();
            }

            if (user.PasswordHash != HashPassword(Password))
            {
                ModelState.AddModelError("Password", "Mật khẩu không đúng.");
                return Page();
            }
            if (user.IsLocked)
            {
                ModelState.AddModelError("", "Tài khoản của bạn đã bị khóa.");
                return Page();
            }

            // Lưu thông tin vào session
                HttpContext.Session.SetUserId(user.Id);
            HttpContext.Session.SetUserRole(user.Role);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Useremail", user.Email);


            // Chuyển hướng đến trang chính
            TempData["SuccessMessage"] = "🎉 Đăng nhập thành công. Chào mừng trở lại!";
            return RedirectToPage("/Index");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            return BitConverter
                .ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)))
                .Replace("-", "")
                .ToLower();
        }
    }
}