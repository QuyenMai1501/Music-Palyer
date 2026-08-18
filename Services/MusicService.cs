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

            var playCounts = _context.Plays
                .Where(p => p.PlayDate.Date >= startOfWeek && p.PlayDate.Date <= today)
                .GroupBy(p => p.SongId)
                .Select(g => new { SongId = g.Key, Count = g.Count() })
                .ToList();

            var likeCounts = _context.Likes
                .Where(l => l.LikeDate.Date >= startOfWeek && l.LikeDate.Date <= today)
                .GroupBy(l => l.SongId)
                .Select(g => new { SongId = g.Key, Count = g.Count() })
                .ToList();

            var scores = new Dictionary<int, int>();
            foreach (var item in playCounts)
            {
                scores[item.SongId] = scores.GetValueOrDefault(item.SongId) + item.Count;
            }
            foreach (var item in likeCounts)
            {
                scores[item.SongId] = scores.GetValueOrDefault(item.SongId) + item.Count;
            }

            return _context.Songs
                .Where(s => !s.IsHidden)
                .ToList()
                .Where(s => scores.ContainsKey(s.Id))
                .OrderByDescending(s => scores.GetValueOrDefault(s.Id))
                .Take(10) // Hiển thị 10 bài hát phổ biến nhất trong tuần
                .ToList();
        }
    }
}