using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MusicPlayer.Data;
using MusicPlayer.Helpers;
using MusicPlayer.Models;

namespace MusicPlayer.Pages.Playlists
{
    public class EditModel(AppDbContext context) : PageModel
    {
        private readonly AppDbContext _context = context;

        [BindProperty]
        public Playlist Playlist { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = HttpContext.Session.GetUserId();
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            Playlist = await _context.Playlists
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (Playlist == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = HttpContext.Session.GetUserId();
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var playlistToUpdate = await _context.Playlists
                .FirstOrDefaultAsync(p => p.Id == Playlist.Id && p.UserId == userId);

            if (playlistToUpdate == null)
            {
                return NotFound();
            }

            playlistToUpdate.Name = Playlist.Name;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tên danh sách phát nhạc đã được cập nhật thành công.";
            return RedirectToPage("/Playlists/Index");
        }
    }
}