using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicPlayer.Data;
using MusicPlayer.Helpers;
using MusicPlayer.Models;
using MusicPlayer.Services;

namespace MusicPlayer.Pages.Admin
{
    // Soạn lời bài hát (đồng bộ theo thời gian) ngay trên web, thay cho công cụ bên thứ ba.
    public class EditLyricsModel(AppDbContext context, MediaStorage mediaStorage) : PageModel
    {
        private readonly AppDbContext _context = context;
        private readonly MediaStorage _mediaStorage = mediaStorage;

        public Song? Song { get; set; }
        public string LyricsText { get; set; } = string.Empty;

        [BindProperty]
        public string LinesJson { get; set; } = string.Empty;

        public IActionResult OnGet(int id)
        {
            if (HttpContext.Session.GetUserRole() != "Admin")
            {
                return RedirectToPage("/Error");
            }

            Song = _context.Songs.FirstOrDefault(s => s.Id == id);
            if (Song == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(Song.LyricsPath))
            {
                LyricsText = _mediaStorage.ReadAllText(Song.LyricsPath);
            }

            return Page();
        }

        // Stream audio để soạn lời — không ghi lượt nghe (Play) để không làm phồng xếp hạng tuần.
        public IActionResult OnGetAudio(int id)
        {
            var song = _context.Songs.FirstOrDefault(s => s.Id == id);
            if (song == null)
            {
                return NotFound();
            }

            var filePath = _mediaStorage.GetPhysicalPath(song.FilePath);
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            Response.Headers["Accept-Ranges"] = "bytes";
            return PhysicalFile(filePath, "audio/mpeg");
        }

        public IActionResult OnPostSave(int id)
        {
            if (HttpContext.Session.GetUserRole() != "Admin")
            {
                return RedirectToPage("/Error");
            }

            Song = _context.Songs.FirstOrDefault(s => s.Id == id);
            if (Song == null)
            {
                return NotFound();
            }

            var lines = ParseLines(LinesJson);
            if (lines == null)
            {
                ModelState.AddModelError("", "Dữ liệu lời bài hát không hợp lệ.");
                return Page();
            }

            // Giữ lại nội dung người dùng vừa nhập nếu lưu lỗi phải quay lại trang.
            LyricsText = BuildLrc(lines);

            if (string.IsNullOrWhiteSpace(LyricsText))
            {
                // Không còn dòng nào: xoá lời bài hát.
                if (!string.IsNullOrEmpty(Song.LyricsPath) && _mediaStorage.Exists(Song.LyricsPath))
                {
                    var oldPath = _mediaStorage.GetPhysicalPath(Song.LyricsPath);
                    if (!string.IsNullOrEmpty(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }
                Song.LyricsPath = null;
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Đã xoá lời bài hát.";
                return RedirectToPage("/Admin/ManageSongs");
            }

            string fileName;
            if (!string.IsNullOrEmpty(Song.LyricsPath) && _mediaStorage.Exists(Song.LyricsPath))
            {
                // Ghi đè đúng file cũ để không sinh file rác.
                fileName = Path.GetFileName(Song.LyricsPath.TrimEnd('/', '\\'));
            }
            else
            {
                string baseName = SanitizeFileName(Song.Title);
                fileName = $"{Guid.NewGuid()}_{(string.IsNullOrEmpty(baseName) ? "lyric" : baseName)}.lrc";
            }

            Song.LyricsPath = _mediaStorage.SaveText("lyrics", fileName, LyricsText);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Đã lưu lời bài hát.";
            return RedirectToPage("/Admin/ManageSongs");
        }

        private sealed class LyricLine
        {
            public double Time { get; set; }
            public string Text { get; set; } = string.Empty;
        }

        private static List<LyricLine>? ParseLines(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<LyricLine>();
            }
            try
            {
                var lines = JsonSerializer.Deserialize<List<LyricLine>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return lines ?? new List<LyricLine>();
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static string BuildLrc(List<LyricLine> lines)
        {
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                string text = (line.Text ?? string.Empty).Trim();
                if (text.Length == 0 || line.Time < 0)
                {
                    continue;
                }
                var time = TimeSpan.FromSeconds(line.Time);
                sb.Append('[')
                  .Append(time.Minutes.ToString("00")).Append(':')
                  .Append(time.Seconds.ToString("00")).Append('.')
                  .Append((time.Milliseconds / 10).ToString("00"))
                  .Append(']').Append(text).Append('\n');
            }
            return sb.ToString();
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var chars = name.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
            return new string(chars).Trim();
        }
    }
}