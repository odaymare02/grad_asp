using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduate.DAL.Models
{
    public class ChatMessage
    {
        public int ChatMessageId { get; set; }
        public int ChatSessionId { get; set; }
        public ChatSession ChatSession { get; set; }
        public string Major { get; set; }
        public string Role { get; set; } // "user" or "bot"
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
