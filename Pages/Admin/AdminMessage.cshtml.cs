using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MusicPlayer.Data;
using MusicPlayer.Models;

namespace MusicPlayer.Pages.Admin
{
    public class AdminMessageModel(AppDbContext context) : PageModel
    {
        private readonly AppDbContext _context = context;

        public List<Message> Messages { get; set; } = new();

        [BindProperty]
        [Required(ErrorMessage = "Phản hồi không được để trống.")]
        public Message AdminReplyMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 4;
        public int TotalPages { get; set; }

        

        public async Task<IActionResult> OnGetAsync()
        {
            var totalCount = await _context.Messages.CountAsync();

            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            Messages = await _context.Messages
                .OrderByDescending(m => m.CreatedAt)
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int messageId)
        {
            var replyKey = $"AdminReply_{messageId}";
            var reply = Request.Form[replyKey];
            if (string.IsNullOrWhiteSpace(reply))
            {
                ModelState.AddModelError("AdminReply", "Phản hồi không được để trống.");
                await OnGetAsync(); // load lại danh sách phân trang
                return Page();
            }


            var message = _context.Messages.Find(messageId);
            if (message == null)
            {
                ModelState.AddModelError("", "Không tìm thấy phản hồi.");
                await OnGetAsync(); // load lại danh sách phân trang
                return Page();
            }


            // Lưu phản hồi của Admin vào database
            message.AdminReply = reply;
            message.AdminReplyAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bạn đã trả lời phản hồi!";

            await OnGetAsync(); // load lại danh sách phân trang
            return Page();
        }
    }
}