using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MusicPlayer.Data;
using MusicPlayer.Helpers;
using MusicPlayer.Models;
using TagLib;

namespace MusicPlayer.Pages.Playlists
{
    public class MusicPlayerModel(AppDbContext context) : PageModel
    {
        private readonly AppDbContext _context = context;

        public Playlist? Playlist { get; set; }
        public Song? CurrentSong { get; set; }
        public Dictionary<int, TimeSpan> SongDurations { get; set; } = new Dictionary<int, TimeSpan>();

        public async Task<IActionResult> OnGetAsync(int playlistId, int? songId)
        {
            var userId = HttpContext.Session.GetUserId();
            if (userId == null) return RedirectToPage("/Account/Login");

            Playlist = await _context.Playlists
                .Include(p => p.PlaylistSongs)
                .ThenInclude(ps => ps.Song)
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

            if (Playlist == null || Playlist.PlaylistSongs == null || !Playlist.PlaylistSongs.Any())
                return RedirectToPage("/Error");

            CurrentSong = songId.HasValue
                ? Playlist.PlaylistSongs.FirstOrDefault(ps => ps.Song.Id == songId.Value)?.Song
                : Playlist.PlaylistSongs.FirstOrDefault()?.Song;

            if (CurrentSong == null) return RedirectToPage("/Error");

            // Chuẩn hóa FilePath, ImagePath và xử lý Artist
            CurrentSong.FilePath = CurrentSong.FilePath.StartsWith("/") 
                ? CurrentSong.FilePath 
                : $"/{CurrentSong.FilePath}";
            CurrentSong.ImagePath = string.IsNullOrEmpty(CurrentSong.ImagePath) 
                ? "/images/default.jpg" 
                : CurrentSong.ImagePath.StartsWith("/") 
                    ? CurrentSong.ImagePath 
                    : $"/images/{CurrentSong.ImagePath}";
            CurrentSong.Artist = string.IsNullOrEmpty(CurrentSong.Artist) 
                ? "Không rõ" 
                : CurrentSong.Artist;
            CurrentSong.Title = string.IsNullOrEmpty(CurrentSong.Title) 
                ? "Không rõ" 
                : CurrentSong.Title;

            Console.WriteLine($"CurrentSong: Title={CurrentSong.Title}, Artist={CurrentSong.Artist}"); // Debug: Kiểm tra CurrentSong

            // Trích xuất thời lượng cho tất cả bài hát
            foreach (var playlistSong in Playlist.PlaylistSongs)
            {
                try
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", playlistSong.Song.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        using (var file = TagLib.File.Create(filePath))
                        {
                            SongDurations[playlistSong.Song.Id] = file.Properties.Duration;
                        }
                    }
                    else
                    {
                        SongDurations[playlistSong.Song.Id] = TimeSpan.Zero;
                        Console.WriteLine($"File not found: {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi đọc file {playlistSong.Song.FilePath}: {ex.Message}");
                    SongDurations[playlistSong.Song.Id] = TimeSpan.Zero;
                }
                // Đảm bảo Title, Artist không rỗng, lấy từ cơ sở dữ liệu
                playlistSong.Song.Title = string.IsNullOrEmpty(playlistSong.Song.Title) 
                    ? "Không rõ" 
                    : playlistSong.Song.Title;
                playlistSong.Song.Artist = string.IsNullOrEmpty(playlistSong.Song.Artist) 
                    ? "Không rõ" 
                    : playlistSong.Song.Artist;
                Console.WriteLine($"Song {playlistSong.Song.Id}: Title={playlistSong.Song.Title}, Artist={playlistSong.Song.Artist}"); // Debug: Kiểm tra từng bài hát
            }

            return Page();
        }
    }
}