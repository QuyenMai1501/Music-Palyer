using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicPlayer.Data;

namespace MusicPlayer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SearchController(AppDbContext context)
        {
            _context = context;
        }

        // Gọi ví dụ: GET /api/Search/{query}
        [HttpGet("{query}")]
        public async Task<IActionResult> SearchSong(string query)
        {
            var song = await _context.Songs
                .AsNoTracking()
                .FirstOrDefaultAsync(s =>
                    EF.Functions.Like(s.Title, $"%{query}%") ||
                    EF.Functions.Like(s.Artist, $"%{query}%")); // Tìm kiếm cả theo ca sĩ

            if (song == null)
            {
                string contactUrl = $"/Contact?messaContent={Uri.EscapeDataString($"Thêm bài \"{query}\" vào hệ thống.")}";

                return Ok(new
                {
                    found = false,
                    message = "🚫 Hệ thống chưa có bài hát hoặc ca sĩ này. Vui lòng liên hệ Admin.",
                    contactLink = contactUrl
                });
            }

            return Ok(new
            {
                found = true,
                Title = song.Title,
                Artist = song.Artist,
                FilePath = song.FilePath,
                playLink = $"/Music/Player?songId={song.Id}"
            });
        }
    }
}