using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicPlayer.Data;
using MusicPlayer.Models;

namespace MusicPlayer.Pages.Account
{
    public class RegisterModel(AppDbContext db) : PageModel
    {
        private readonly AppDbContext _db = db;


        [BindProperty]
        [Required(ErrorMessage = "Tên người dùng không được để trống.")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Tên người dùng phải từ 3 đến 20 ký tự.")]
        public string Username { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Vui lòng nhập email hợp lệ.")]
        public required string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 20 ký tự.")]
        public required string Password { get; set; }

        // Xử lý khi submit form đăng ký
        public IActionResult OnPost()
        {
            // Nếu có lỗi, trả về trang hiện tại với thông báo lỗi
            if (!ModelState.IsValid)
            {
                // Ghi log để debug các lỗi ModelState
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi: {error.ErrorMessage}");
                }
                return Page();
            }

            // Tạo user mới và lưu vào database
            var newUser = new User
            {
                Username = Username,
                Email = Email,
                PasswordHash = HashPassword(Password),
                Role = "User",
                CreateDate = DateTime.Now
            };
            _db.Users.Add(newUser);
            _db.SaveChanges();

            // Chuyển hướng đến trang đăng nhập sau khi đăng ký thành công
            TempData["SuccessMessage"] = "Đăng ký thành công. Vui lòng đăng nhập";
            return RedirectToPage("/Account/Login");
        }

        // Hàm mã hóa mật khẩu bằng SHA256
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