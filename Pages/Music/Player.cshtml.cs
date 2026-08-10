using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicPlayer.Data;
using MusicPlayer.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using TagLib;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using MusicPlayer.Helpers;

namespace MusicPlayer.Pages.Music
{
    public class PlayerModel(AppDbContext context, IWebHostEnvironment environment) : PageModel
    {
        private readonly AppDbContext _context = context;
        private readonly IWebHostEnvironment _environment = environment;

        public Song? Song { get; set; }
        public string SongDuration { get; set; } = "00:00";
        public string LrcContent { get; set; } = string.Empty;
        public bool HasUserLiked { get; set; }
        public int LikeCount { get; set; }

        public int? PrevSongId { get; set; }
        public int? NextSongId { get; set; }

        public IActionResult OnGet(int songId)
        {
            Song = _context.Songs.FirstOrDefault(s => s.Id == songId);
            if (Song == null)
            {
                return RedirectToPage("/Music/Index");
            }

            if (string.IsNullOrEmpty(Song.ImagePath))
            {
                Song.ImagePath = "/images/default.jpg";
            }
            else
            {
                string normalizedImagePath = Song.ImagePath;
                if (!normalizedImagePath.StartsWith("/images/"))
                {
                    normalizedImagePath = $"/images/{Path.GetFileName(Song.ImagePath)}";
                }

                var imagePath = Path.Combine(_environment.WebRootPath, normalizedImagePath.TrimStart('/', '\\'));
                if (!System.IO.File.Exists(imagePath))
                {
                    Song.ImagePath = "/images/default.jpg";
                }
                else
                {
                    Song.ImagePath = normalizedImagePath;
                }
            }

            if (!string.IsNullOrEmpty(Song.LyricsPath))
            {
                var lrcFilePath = Path.Combine(_environment.WebRootPath, Song.LyricsPath.TrimStart('/', '\\'));
                if (System.IO.File.Exists(lrcFilePath))
                {
                    LrcContent = System.IO.File.ReadAllText(lrcFilePath);
                }
                else
                {
                    LrcContent = "";
                }
            }

            var userId = HttpContext.Session.GetUserId();
            HasUserLiked = _context.Likes.Any(l => l.SongId == songId && l.UserId == userId);
            LikeCount = GetLikeCount(songId);
            SongDuration = GetSongDuration(songId);

            var allSongIds = _context.Songs.OrderBy(s => s.Id).Select(s => s.Id).ToList();
            var currentIndex = allSongIds.IndexOf(songId);
            PrevSongId = currentIndex > 0 ? allSongIds[currentIndex - 1] : (int?)null;
            NextSongId = currentIndex < allSongIds.Count - 1 ? allSongIds[currentIndex + 1] : (int?)null;

            return Page();
        }

        public IActionResult OnGetPlay(int songId)
        {
            var song = _context.Songs.Find(songId);
            if (song == null)
            {
                return NotFound();
            }

            _context.Plays.Add(new Play { SongId = songId, PlayDate = DateTime.UtcNow });
            _context.SaveChanges();

            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            var filePath = Path.Combine(_environment.WebRootPath, song.FilePath.TrimStart('/', '\\'));
            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var response = PhysicalFile(filePath, "audio/mpeg");
            Response.Headers["Accept-Ranges"] = "bytes";
            return response;
        }


        public JsonResult OnPostToggleLike(int songId)
        {
            var userId = HttpContext.Session.GetUserId();
            var existingLike = _context.Likes.FirstOrDefault(l => l.SongId == songId && l.UserId == userId);
            bool hasUserLiked;
            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
                hasUserLiked = false;
            }
            else
            {
                _context.Likes.Add(new Like
                {
                    SongId = songId,
                    UserId = userId,
                    LikeDate = DateTime.UtcNow
                });
                hasUserLiked = true;
            }

            _context.SaveChanges();
            int likeCount = _context.Likes.Count(l => l.SongId == songId);
            return new JsonResult(new { hasUserLiked, likeCount });
        }

        public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            base.OnPageHandlerExecuting(context);
        }

        public int GetLikeCount(int songId)
        {
            return _context.Likes.Count(l => l.SongId == songId);
        }

        private string GetSongDuration(int songId)
        {
            var song = _context.Songs.Find(songId);
            if (song == null)
            {
                return "00:00";
            }

            var filePath = Path.Combine(_environment.WebRootPath, song.FilePath.TrimStart('/', '\\'));
            if (!System.IO.File.Exists(filePath))
            {
                return "00:00";
            }

            try
            {
                using (var file = TagLib.File.Create(filePath))
                {
                    var duration = file.Properties.Duration;
                    return $"{duration.Minutes:D2}:{duration.Seconds:D2}";
                }
            }
            catch
            {
                return "00:00";
            }
        }
    }
}