using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MusicPlayer.Data;
using MusicPlayer.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;
using MusicPlayer.Models;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;

namespace MusicPlayer.Pages.Admin
{
    public class AddSongModel(AppDbContext context, IWebHostEnvironment environment) : PageModel
    {
        private readonly AppDbContext _context = context;
        private readonly IWebHostEnvironment _environment = environment;

        [BindProperty]
        [Required(ErrorMessage = "Tên bài hát không được để trống.")]
        [StringLength(100, ErrorMessage = "Tên bài hát không được dài quá 100 ký tự.")]
        public required string Title { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Tên ca sĩ không được để trống.")]
        [StringLength(50, ErrorMessage = "Tên ca sĩ không được dài quá 50 ký tự.")]

        public required string Artist { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng chọn file nhạc.")]

        public required IFormFile UploadFile { get; set; }
        
        [BindProperty]
        public IFormFile? UploadImage { get; set; }
        public IFormFile? UploadLyrics { get; set; }

        public IActionResult OnGet()
        {
            var userRole = HttpContext.Session.GetUserRole();
            if (userRole != "Admin")
            {
                return RedirectToPage("/Error");
            }
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

            // Kiểm tra định dạng và kích thước file trước khi lưu
            if (!IsAllowedAudio(UploadFile))
            {
                ModelState.AddModelError("UploadFile", "Chỉ chấp nhận file nhạc định dạng .mp3 (tối đa 100MB).");
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

            string? fileName = null;
            string? imageFileName = null;
            string? lyricsFileName = null;

            if (UploadFile != null && UploadFile.Length > 0)
            {
                var folderPath = Path.Combine(_environment.WebRootPath, "music");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                fileName = Path.GetFileName(UploadFile.FileName).Trim();
                var filePath = Path.Combine(folderPath, fileName);

                try
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        UploadFile.CopyTo(stream);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi khi lưu file: {ex.Message}");
                    ModelState.AddModelError("", "Không thể lưu file nhạc");
                    return Page(); // Quay lại trang nếu có lỗi
                }
            }

            if (UploadImage != null && UploadImage.Length > 0)
            {
                var imageFolderPath = Path.Combine(_environment.WebRootPath, "images");
                if (!Directory.Exists(imageFolderPath)) Directory.CreateDirectory(imageFolderPath);

                imageFileName = Path.GetFileName(UploadImage.FileName).Trim();
                var imageFilePath = Path.Combine(imageFolderPath, imageFileName);
                try
                {
                    using (var stream = new FileStream(imageFilePath, FileMode.Create))
                    {
                        UploadImage.CopyTo(stream);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi khi lưu file ảnh: {ex.Message}");
                    ModelState.AddModelError("", "Không thể lưu file ảnh");
                    return Page();
                }
            }

            if (UploadLyrics != null && UploadLyrics.Length > 0)
            {
                var lyricsFolderPath = Path.Combine(_environment.WebRootPath, "lyrics");
                if (!Directory.Exists(lyricsFolderPath)) Directory.CreateDirectory(lyricsFolderPath);

                // SỬA: Tạo tên file lời duy nhất
                lyricsFileName = $"{Guid.NewGuid()}_{Path.GetFileName(UploadLyrics.FileName).Trim()}";
                var lyricsFilePath = Path.Combine(lyricsFolderPath, lyricsFileName);
                try
                {
                    using (var stream = new FileStream(lyricsFilePath, FileMode.Create))
                    {
                        UploadLyrics.CopyTo(stream);
                    }
                    Console.WriteLine($"Lưu file lời: {lyricsFilePath}"); // SỬA: Thêm log
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi khi lưu file lời: {ex.Message}");
                    ModelState.AddModelError("", "Không thể lưu file lời");
                    return Page();
                }
            }

            // Đảm bảo không lặp "/music/" trong đường dẫn
            var song = new Song
            {
                Title = Title,
                Artist = Artist,
                FilePath = fileName != null ? $"/music/{fileName}" : "/music/default.mp3",
                ImagePath = imageFileName != null ? $"/images/{imageFileName}" : "/images/default.png",
                LyricsPath = lyricsFileName != null ? $"/lyrics/{lyricsFileName}" : null,
                CreateDate = DateTime.Now
            };
        

            _context.Songs.Add(song);

            try
            {
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Thêm bài hát thành công";
                return RedirectToPage("/Admin/ManageSongs");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi lưu vào cơ sở dữ liệu: {ex.Message}");
                ModelState.AddModelError("", "Lỗi khi lưu bài hát vào cơ sở dữ liệu.");
                return Page();
            }
            
        }

        private static bool IsAllowedAudio(IFormFile? file)
        {
            if (file == null || file.Length == 0) return true;
            if (file.Length > 100L * 1024 * 1024) return false;
            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            bool validContentType = file.ContentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase)
                || file.ContentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase);
            return ext == ".mp3" && validContentType;
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