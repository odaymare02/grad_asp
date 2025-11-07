using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduate.DAL.Dto.Request
{
    public class SaveCompletedDto
    {
        public string? UserId { get; set; }
        public string Major { get; set; }
        public List<string> CompletedCourseCodes { get; set; } = new();
    }
}
