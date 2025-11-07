using Graduate.DAL.Data;
using Graduate.DAL.Dto.Request;
using Graduate.DAL.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Graduate.BLL.Services.Interfaces;


namespace Graduate.BLL.Services.Clasess
{
    public class CourseService:ICourseService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;
        public CourseService(IWebHostEnvironment env, ApplicationDbContext context)
        {
            _env = env;
            _context = context;
        }

        public async Task<JsonElement?> GetCoursesByMajorAsync(string major, string userId)
        {
            var filePath = Path.Combine(_env.ContentRootPath, "Data", "majors_plan_dynamic_info.json");
            var json = await File.ReadAllTextAsync(filePath);

            var root = JsonSerializer.Deserialize<JsonElement>(json);
            if (!root.TryGetProperty(major, out var majorData))
                return null;

            var completedCourses = await _context.CompletedCourses
                .Where(c => c.UserId == userId && c.Major == major && c.IsCompleted)
                .Select(c => c.CourseCode)
                .ToListAsync();

            var majorDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(majorData.GetRawText());
            var newMajorDict = new Dictionary<string, object>();

            foreach (var category in majorDict)
            {
                var categoryData = category.Value;
                var hours = categoryData.GetProperty("عدد الساعات المطلوبة").GetInt32();
                var courses = categoryData.GetProperty("المساقات").EnumerateArray().Select(c =>
                {
                    var code = c.GetProperty("رقم المساق").GetString();
                    var isCompleted = completedCourses.Contains(code);

                    var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(c.GetRawText());
                    dict["IsCompleted"] = isCompleted;
                    return dict;
                }).ToList();

                newMajorDict[category.Key] = new
                {
                    عدد_الساعات_المطلوبة = hours,
                    المساقات = courses
                };
            }

            var jsonString = JsonSerializer.Serialize(newMajorDict);
            return JsonSerializer.Deserialize<JsonElement>(jsonString);
        }
        public async Task SaveCompletedCoursesAsync(SaveCompletedDto request)
        {
            // delete old 
            var old = _context.CompletedCourses
                .Where(c => c.UserId == request.UserId && c.Major == request.Major);
            _context.CompletedCourses.RemoveRange(old);

            // add new
            foreach (var code in request.CompletedCourseCodes)
            {
                _context.CompletedCourses.Add(new CompletedCourses
                {
                    UserId = request.UserId,
                    Major = request.Major,
                    CourseCode = code,
                    IsCompleted = true
                });
            }

            await _context.SaveChangesAsync();
        }
        public async Task<List<string>> GetCompletedCourseNamesAsync(string userId, string major)
        {
            var filePath = Path.Combine(_env.ContentRootPath, "Data", "majors_plan_dynamic_info.json");
            var json = await File.ReadAllTextAsync(filePath);
            var root = JsonSerializer.Deserialize<JsonElement>(json);

            if (!root.TryGetProperty(major, out var majorData))
                return new List<string>();

            var completedCodes = await _context.CompletedCourses
                .Where(c => c.UserId == userId && c.Major == major && c.IsCompleted)
                .Select(c => c.CourseCode)
                .ToListAsync();

            var majorDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(majorData.GetRawText());
            var completedNames = new List<string>();

            foreach (var category in majorDict)
            {
                var courses = category.Value.GetProperty("المساقات").EnumerateArray();
                foreach (var course in courses)
                {
                    var code = course.GetProperty("رقم المساق").GetString();
                    if (completedCodes.Contains(code))
                    {
                        var name = course.GetProperty("اسم المساق").GetString();
                        completedNames.Add(name);
                    }
                }
            }

            return completedNames;
        }

    }
}
