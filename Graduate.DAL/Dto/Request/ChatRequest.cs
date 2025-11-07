using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduate.DAL.Dto.Request
{
    public class ChatRequest
    {
        public int SessionId { get; set; }
        public string Role { get; set; } // "user" or "bot"
        public string Content { get; set; }
        public string Major { get; set; } // التخصص
        public List<string>? CompletedCourses { get; set; }
    }
}
