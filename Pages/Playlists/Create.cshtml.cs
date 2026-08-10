using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicPlayer.Data;
using MusicPlayer.Helpers;
using MusicPlayer.Models;

namespace MusicPlayer.Pages.Playlists
{
    public class CreateModel(AppDbContext context) : PageModel
    {
        private readonly AppDbContext _context = context;

        [BindProperty]
        public Playlist Playlist { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (HttpContext.Session.GetUserId() == null)
            {
                return RedirectToPage("/Account/Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var userId = HttpContext.Session.GetUserId();
            if (userId == null) return RedirectToPage("/Account/Login");

            Playlist.UserId = userId.Value;
            _context.Playlists.Add(Playlist);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Playlists/Index");
        }
    }
}