using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduate.DAL.Dto.Request
{
    public class ChatSessionDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ChatMessageDto> Messages { get; set; } = new List<ChatMessageDto>();
    }
}
