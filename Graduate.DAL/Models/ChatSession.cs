using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduate.DAL.Models
{
    public class ChatSession
    {
        public int ChatSessionId { get; set; }
        public string ?Title { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<ChatMessage> Messages { get; set; }
    }
}
