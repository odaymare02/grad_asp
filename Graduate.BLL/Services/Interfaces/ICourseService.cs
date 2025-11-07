using Graduate.DAL.Dto.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Graduate.BLL.Services.Interfaces
{
    public interface ICourseService
    {
        Task<JsonElement?> GetCoursesByMajorAsync(string major, string userId);
        Task SaveCompletedCoursesAsync(SaveCompletedDto request);
        Task<List<string>> GetCompletedCourseNamesAsync(string userId, string major);



    }
}
