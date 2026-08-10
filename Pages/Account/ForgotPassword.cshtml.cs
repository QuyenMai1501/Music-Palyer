using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicPlayer.Data;
using Microsoft.AspNetCore.WebUtilities;

namespace MusicPlayer.Pages.Account
{
    public class ForgotPasswordModel(AppDbContext context) : PageModel
    {
        private readonly AppDbContext _context = context;

        [BindProperty]
        [Required(ErrorMessage = "Email không được để trống.")]
        public string Email { get; set; } = string.Empty;

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            var user = _context.Users.FirstOrDefault(u => u.Email == Email);
            if (user == null)
            {
                TempData["EmailExists"] = false;
                return RedirectToPage("/Account/ForgotPasswordConfirmation");
            }

            using var rng = RandomNumberGenerator.Create();
            byte[] tokenData = new byte[32];
            rng.GetBytes(tokenData);
            string token = WebEncoders.Base64UrlEncode(tokenData);

            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.Now.AddHours(1);
            _context.SaveChanges();

            string resetLink = Url.Page("/Account/ResetPassword", null, new { token }, Request.Scheme);
            TempData["ResetLink"] = resetLink;
            TempData["EmailExists"] = true;

            return RedirectToPage("/Account/ForgotPasswordConfirmation");
        }
    }
}