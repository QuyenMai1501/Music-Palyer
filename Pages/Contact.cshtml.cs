using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicPlayer.Data;
using MusicPlayer.Helpers;
using MusicPlayer.Models;

namespace MusicPlayer.Pages
{
    public class ContactModel(AppDbContext context) : PageModel
    {
        private readonly AppDbContext _context = context;

        [BindProperty]
        public Message Message { get; set; }
        public List<Message> UserMessages { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = HttpContext.Session.GetUserId();
            if (userId == null)
            {
                return RedirectToPage("/Account/Login", new { returnUrl = "/Contact" });
            }

            string username = HttpContext.Session.GetString("Username");
            string useremail = HttpContext.Session.GetString("Useremail");

            UserMessages = _context.Messages
                .Where(m => m.UserEmail == useremail)
                .OrderByDescending(m => m.CreatedAt)
                .ToList() ?? new List<Message>();

            Message = new Message
            {
                UserName = username,
                UserEmail = useremail
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Lỗi: {error.ErrorMessage}");
                }
                return Page();
            }

            try
            {
                Console.WriteLine($"Lưu phản hồi từ: {Message.UserName} - {Message.UserEmail}");
                _context.Messages.Add(Message);
                await _context.SaveChangesAsync();
                Console.WriteLine("Lưu thành công!");


                TempData["SuccessMessage"] = "Phản hồi của bạn đã được gửi!";
                return RedirectToPage("/Contact");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lưu dữ liệu: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Chi tiết lỗi: {ex.InnerException.Message}");
                }

                ModelState.AddModelError("", $"Lỗi khi lưu dữ liệu: {ex.Message}");
                return Page();
            }
        }
    }
}