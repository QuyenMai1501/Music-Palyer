using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicPlayer.Data;
using MusicPlayer.Helpers;
using MusicPlayer.Services;

namespace MusicPlayer.Pages.Account
{
    public class LoginModel(AppDbContext db, PasswordService passwordService) : PageModel
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

            if (HttpContext.Session.IsLoginLocked())
            {
                ModelState.AddModelError("", "Quá nhiều lần đăng nhập sai. Vui lòng thử lại sau 15 phút.");
                return Page();
            }

            // Kiểm tra thông tin đăng nhập
            var user = _db.Users.FirstOrDefault(u => u.Username == Username);
            if (user == null)
            {
                HttpContext.Session.RecordLoginFailure();
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return Page();
            }

            var passwordCheck = passwordService.CheckPassword(Password, user.PasswordHash);
            if (passwordCheck == PasswordCheckResult.Failed)
            {
                HttpContext.Session.RecordLoginFailure();
                ModelState.AddModelError("Password", "Mật khẩu không đúng.");
                return Page();
            }

            // Mật khẩu hợp lệ: xóa bộ đếm thử sai
            HttpContext.Session.ClearLoginFailures();

            // Nâng cấp hash cũ (SHA-256) lên PBKDF2
            if (passwordCheck == PasswordCheckResult.SuccessRehash)
            {
                user.PasswordHash = passwordService.HashPassword(Password);
                _db.SaveChanges();
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
    }
}