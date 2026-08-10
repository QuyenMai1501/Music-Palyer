namespace MusicPlayer.Models
{
    public class Song
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Artist { get; set; }
        public required string FilePath { get; set; }
        public required string ImagePath { get; set; }
        public string? LyricsPath { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime CreateDate { get; set; }
        public List<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
        public bool IsHidden { get; set; } = false;
    }
}