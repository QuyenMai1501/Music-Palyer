using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicPlayer.Data;
using MusicPlayer.Helpers;
using MusicPlayer.Models;

namespace MusicPlayer.Pages.Admin
{
    public class ManageSongsModel(AppDbContext context) : PageModel
    {
        private readonly AppDbContext _context = context;

        public List<Song> Songs { get; set; } = new List<Song>();

        [BindProperty(SupportsGet = true)]
        public int PageIndex {get; set;} = 1;
        public int PageSize {get; set;} = 5;
        public int TotalPages {get; set;} 


        public IActionResult OnGet()
        {
            var userRole = HttpContext.Session.GetUserRole();
            if (userRole != "Admin")
            {
                return RedirectToPage("/Error");
            }

            var totalSongs = _context.Songs.Count();
            TotalPages = (int)Math.Ceiling(totalSongs / (double)PageSize);
            
            
            Songs = _context.Songs
                .OrderByDescending(s => s.Id)
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Page();
        }

        public IActionResult OnPostHide(int id)
        {
            var song = _context.Songs.FirstOrDefault(s => s.Id == id);
            if (song == null)
            {
                return NotFound();
            }
            song.IsHidden = true;
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Đã ẩn bài hát";
            return RedirectToPage();
        }

        public IActionResult OnPostUnhide(int id)
        {
            var song = _context.Songs.FirstOrDefault(s => s.Id == id);
            if (song == null)
            {
                return NotFound();
            }
            song.IsHidden = false;
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Đã hiện bài hát";
            return RedirectToPage();
        }
    }
}