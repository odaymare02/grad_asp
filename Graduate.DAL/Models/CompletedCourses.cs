using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduate.DAL.Models
{
    public class CompletedCourses
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Major { get; set; }
        public string CourseCode { get; set; }
        public bool IsCompleted { get; set; } = false;
    }
}
