using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicPlayer.Data;
using MusicPlayer.Services;

namespace MusicPlayer.Pages.Account
{
    public class ResetPasswordModel(AppDbContext context, PasswordService passwordService) : PageModel
    {
        private readonly AppDbContext _context = context;

        [BindProperty(SupportsGet = true)]
        public string Token { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 20 ký tự.")]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
        [Compare(nameof(Password), ErrorMessage = "Xác nhận mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public bool PasswordResetSuccess { get; set; }
        public IActionResult OnGet(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token không hợp lệ.");
            }
            Token = token;
            return Page();
        }

        public IActionResult OnPost()
        {
            if (Password != ConfirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu và xác nhận mật khẩu không trùng khớp.");
                return Page();
            }

            var user = _context.Users.FirstOrDefault(u =>
                u.ResetToken == Token && u.ResetTokenExpiry != null && u.ResetTokenExpiry > DateTime.UtcNow);
            if (user == null)
            {
                ModelState.AddModelError("", "Token không hợp lệ hoặc đã hết hạn.");
                return Page();
            }

            user.PasswordHash = passwordService.HashPassword(Password);

            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            _context.SaveChanges();

            PasswordResetSuccess = true;
            return Page();
        }

    }
}