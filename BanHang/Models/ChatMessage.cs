using System.ComponentModel.DataAnnotations;

namespace BanHang.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsAdmin { get; set; } = false;
    }
}
