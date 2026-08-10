using System.ComponentModel.DataAnnotations;

namespace MusicPlayer.Models
{
    public class Message
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string UserEmail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung phản hồi.")]
        [StringLength(500, ErrorMessage = "Nội dung quá dài.")]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Admin có thể trả lời phản hồi
        public string? AdminReply { get; set; }
        public DateTime? AdminReplyAt { get; set; }
    }
}