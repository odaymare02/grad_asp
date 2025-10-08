using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduate.DAL.Dto.Response
{
    public class ChatStatsDto
    {
        public int TotalSessions { get; set; }
        public int TotalMessages { get; set; }
        public DateTime LastActivity { get; set; }
    }
}
