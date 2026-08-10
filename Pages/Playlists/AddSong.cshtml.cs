using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MusicPlayer.Data;
using MusicPlayer.Models;

namespace MusicPlayer.Pages.Playlists
{
    public class AddSongModel(AppDbContext context) : PageModel
    {
        private readonly AppDbContext _context = context;

        public Playlist Playlist { get; set; }
        public List<Song> AvailableSongs { get; set; } = new List<Song>();

        [BindProperty]
        public List<int> SelectedSongIds { get; set; } = new List<int>();

        [BindProperty(SupportsGet = true)]
        public int PageIndex {get; set;} = 1;
        public int PageSize {get; set;} = 4;
        public int TotalPages {get; set;} 

        public async Task<IActionResult> OnGetAsync(int playlistId)
        {
            Playlist = await _context.Playlists
                .Include(p => p.PlaylistSongs)
                .ThenInclude(ps => ps.Song)
                .FirstOrDefaultAsync(p => p.Id == playlistId);

            if (Playlist == null) return NotFound();

            var existingSongIds = Playlist.PlaylistSongs.Select(ps => ps.SongId).ToList();

            var songsQuery = _context.Songs
                .Where(s => !existingSongIds.Contains(s.Id))
                .OrderBy(s => s.Title);

            int totalCount = songsQuery.Count();
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            AvailableSongs = await songsQuery
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int playlistId)
        {
            if (SelectedSongIds == null || !SelectedSongIds.Any())
            {
                ModelState.AddModelError("", "Bạn phải chọn ít nhất một bài hát.");
                return Page();
            }

            var playlist = await _context.Playlists
                .Include(p => p.PlaylistSongs)
                .FirstOrDefaultAsync(p => p.Id == playlistId);

            if (playlist == null) return NotFound();

            foreach (var songId in SelectedSongIds)
            {
                if (!playlist.PlaylistSongs.Any(ps => ps.SongId == songId))
                {
                    _context.PlaylistSongs.Add(new PlaylistSong { PlaylistId = playlist.Id, SongId = songId });
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã thêm bài hát vào playlist!";
            return RedirectToPage("/Playlists/Details", new { id = playlist.Id });
        }
    }
}