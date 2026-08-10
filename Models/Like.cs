namespace MusicPlayer.Models
{
    public class Like
    {
        public int Id { get; set; }
        public int SongId { get; set; }
        public int? UserId { get; set; }
        public DateTime LikeDate { get; set; }
    }
}