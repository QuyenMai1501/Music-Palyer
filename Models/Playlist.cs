namespace MusicPlayer.Models
{
    public class Playlist
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int UserId { get; set; }
        public List<PlaylistSong> PlaylistSongs { get; set; } = new List<PlaylistSong>();
    }
}