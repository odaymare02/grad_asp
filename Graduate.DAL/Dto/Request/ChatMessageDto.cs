using Graduate.DAL.Dto.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduate.DAL.Dto.Request
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public string Role { get; set; }

        public int ChatSessionId { get; set; }
        public RagResponse BotResponse { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }= DateTime.Now;
    }
}
