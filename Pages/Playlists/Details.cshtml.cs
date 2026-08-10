using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MusicPlayer.Data;
using MusicPlayer.Helpers;
using MusicPlayer.Models;

namespace MusicPlayer.Pages.Playlists
{
    public class DetailsModel(AppDbContext context) : PageModel
    {
        private readonly AppDbContext _context = context;

        [BindProperty]
        public Playlist? Playlist { get; set; }

        public List<PlaylistSong> PlaylistSongsPaged { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        
        public async Task<IActionResult> OnGetAsync(int id, [FromQuery(Name = "page")] int page = 1)
        {
            const int pageSize = 5;

            var userId = HttpContext.Session.GetUserId();
            if (userId == null) return RedirectToPage("/Account/Login");

            Playlist = await _context.Playlists
                .Include(p => p.PlaylistSongs)
                .ThenInclude(ps => ps.Song)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (Playlist == null) return RedirectToPage("/Error");

            var allSongs = Playlist.PlaylistSongs.OrderByDescending(ps => ps.Song.Id).ToList();
            var totalCount = allSongs.Count;

            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            page = Math.Clamp(page, 1, Math.Max(TotalPages, 1));
            CurrentPage = page;

            PlaylistSongsPaged = allSongs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteSelectedSongsAsync(int playlistId, int[] songIds)
        {
            var userId = HttpContext.Session.GetUserId();
            if (userId == null) return RedirectToPage("/Account/Login");

            var playlist = await _context.Playlists
                .Include(p => p.PlaylistSongs)
                .FirstOrDefaultAsync(p => p.Id == playlistId && p.UserId == userId);

            if (playlist == null) return RedirectToPage("/Error");

            if (songIds == null || !songIds.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một bài hát để xóa.";
                return RedirectToPage("/Playlists/Details", new { id = playlistId });
            }

            var playlistSongsToRemove = await _context.PlaylistSongs
                .Where(ps => ps.PlaylistId == playlistId && songIds.Contains(ps.SongId))
                .ToListAsync();

            if (!playlistSongsToRemove.Any())
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài hát nào để xóa.";
                return RedirectToPage("/Playlists/Details", new { id = playlistId });
            }

            _context.PlaylistSongs.RemoveRange(playlistSongsToRemove);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{playlistSongsToRemove.Count} bài hát đã được xóa khỏi playlist";
            return RedirectToPage("/Playlists/Details", new { id = playlistId });
        }
    }
}