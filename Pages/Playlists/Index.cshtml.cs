using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MusicPlayer.Data;
using MusicPlayer.Helpers;
using MusicPlayer.Models;

namespace MusicPlayer.Pages.Playlists
{
    public class IndexModel(AppDbContext context) : PageModel
    {
        private readonly AppDbContext _context = context;

        [BindProperty]
        public List<Playlist>? Playlists { get; set; } = new();
        
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        private const int PageSize = 5;

        public async Task<IActionResult> OnGetAsync([FromQuery(Name = "page")] int page = 1)
        {
            var userId = HttpContext.Session.GetUserId();
            if (userId == null)
            {
                return RedirectToPage("/Account/Login");
            }

            var query = _context.Playlists
                .Include(p => p.PlaylistSongs)
                .ThenInclude(ps => ps.Song)
                .Where(p => p.UserId == userId);

            var totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
            page = Math.Clamp(page, 1, Math.Max(TotalPages, 1));
            CurrentPage = page;


            Playlists = await query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            return Page();
        }
    }
}