using MusicPlayer.Data;
using MusicPlayer.Models;

namespace MusicPlayer.Services
{
    public class MusicService(AppDbContext context)
    {
        private readonly AppDbContext _context = context;

        public int GetPlayCountThisWeek(int songId)
        {
            var today = DateTime.UtcNow.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            return _context.Plays.Count(p => p.SongId == songId && p.PlayDate.Date >= startOfWeek && p.PlayDate.Date <= today);
        }

        public int GetLikeCountThisWeek(int songId)
        {
            var today = DateTime.UtcNow.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            return _context.Likes.Count(l => l.SongId == songId && l.LikeDate.Date >= startOfWeek && l.LikeDate.Date <= today);
        }

        public List<Song> GetTrendingSongsThisWeek()
        {
            var today = DateTime.UtcNow.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);

            return _context.Songs
                .Where(song => !song.IsHidden && _context.Plays.Count(p => p.SongId == song.Id && p.PlayDate.Date >= startOfWeek && p.PlayDate.Date <= today) +
                            _context.Likes.Count(l => l.SongId == song.Id && l.LikeDate.Date >= startOfWeek && l.LikeDate.Date <= today) > 0)
                .OrderByDescending(song => _context.Plays.Count(p => p.SongId == song.Id && p.PlayDate.Date >= startOfWeek && p.PlayDate.Date <= today) +
                                        _context.Likes.Count(l => l.SongId == song.Id && l.LikeDate.Date == today))
                .Take(10) // Hiển thị 10 bài hát phổ biến nhất trong tuần
                .ToList();
        }
    }
}