using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicPlayer.Data;
using MusicPlayer.Models;

namespace MusicPlayer.Pages.Music
{
    public class SongsModel(AppDbContext context) : PageModel
    {
        private readonly AppDbContext _context = context;
        public List<Song> Songs { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public void OnGet([FromQuery(Name = "page")]int page = 1)
        {
            const int pageSize = 10;

            var totalSongs = _context.Songs.Count(song => !song.IsHidden);
            TotalPages = (int)Math.Ceiling(totalSongs / (double)pageSize);
            CurrentPage = page;

            Songs = _context.Songs
                .Where(song => !song.IsHidden)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }
   
}
