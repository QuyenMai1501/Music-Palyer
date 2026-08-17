using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicPlayer.Data;
using MusicPlayer.Helpers;
using MusicPlayer.Models;
using MusicPlayer.Services;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace MusicPlayer.Pages.Admin
{
    public class EditSongModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly MediaStorage _mediaStorage;

        public EditSongModel(AppDbContext context, MediaStorage mediaStorage)
        {
            _context = context;
            _mediaStorage = mediaStorage;
        }

        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Tên bài hát không được để trống.")]
        public required string Title { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Tên ca sĩ không được để trống.")]
        public required string Artist { get; set; }

        // Các file upload tương tự AddSongModel
        [BindProperty]
        public IFormFile? UploadImage { get; set; }

        [BindProperty]
        public IFormFile? UploadLyrics { get; set; }

        public IActionResult OnGet(int id)
        {
            var userRole = HttpContext.Session.GetUserRole();
            if (userRole != "Admin")
            {
                return RedirectToPage("/Error");
            }

            var song = _context.Songs.FirstOrDefault(s => s.Id == id);
            if (song == null)
            {
                return NotFound();
            }

            Id = song.Id;
            Title = song.Title;
            Artist = song.Artist;
            // Bạn có thể hiển thị thông tin file ảnh, lời nếu muốn
            return Page();
        }

        public IActionResult OnPost()
        {
            var userRole = HttpContext.Session.GetUserRole();
            if (userRole != "Admin")
            {
                return RedirectToPage("/Error");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!IsAllowedImage(UploadImage))
            {
                ModelState.AddModelError("UploadImage", "Chỉ chấp nhận file ảnh định dạng .jpg, .jpeg, .png, .webp, .gif (tối đa 5MB).");
                return Page();
            }
            if (!IsAllowedLyrics(UploadLyrics))
            {
                ModelState.AddModelError("UploadLyrics", "Chỉ chấp nhận file lời định dạng .lrc, .txt (tối đa 1MB).");
                return Page();
            }

            var song = _context.Songs.FirstOrDefault(s => s.Id == Id);
            if (song == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin cơ bản
            song.Title = Title;
            song.Artist = Artist;

            // Nếu người dùng tải lên file ảnh mới
            if (UploadImage != null && UploadImage.Length > 0)
            {
                var imageFileName = Path.GetFileName(UploadImage.FileName).Trim();
                try
                {
                    _mediaStorage.SaveFile("images", UploadImage, imageFileName);
                    song.ImagePath = $"/images/{imageFileName}";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi khi lưu file ảnh: {ex.Message}");
                    ModelState.AddModelError("", "Không lưu được file ảnh");
                    return Page();
                }
            }

            // Nếu người dùng tải lên file lời bài hát mới
            if (UploadLyrics != null && UploadLyrics.Length > 0)
            {
                var lyricsFileName = $"{Guid.NewGuid()}_{Path.GetFileName(UploadLyrics.FileName).Trim()}";
                try
                {
                    _mediaStorage.SaveFile("lyrics", UploadLyrics, lyricsFileName);
                    song.LyricsPath = $"/lyrics/{lyricsFileName}";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi khi lưu file lời: {ex.Message}");
                    ModelState.AddModelError("", "Không lưu được file lời bài hát");
                    return Page();
                }
            }

            try
            {
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật bài hát thành công";
                return RedirectToPage("/Admin/ManageSongs", new { PageIndex = 1 }); // Trả về trang quản lý bài hát với trang đầu tiên
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi lưu vào cơ sở dữ liệu: {ex.Message}");
                ModelState.AddModelError("", "Lỗi khi cập nhật bài hát vào cơ sở dữ liệu.");
                return Page();
            }
            
        }

        private static bool IsAllowedImage(IFormFile? file)
        {
            if (file == null || file.Length == 0) return true;
            if (file.Length > 5L * 1024 * 1024) return false;
            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            bool validExt = ext is ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif";
            bool validContentType = file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                || file.ContentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase);
            return validExt && validContentType;
        }

        private static bool IsAllowedLyrics(IFormFile? file)
        {
            if (file == null || file.Length == 0) return true;
            if (file.Length > 1L * 1024 * 1024) return false;
            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            return ext is ".lrc" or ".txt";
        }
    }
}