using Graduate.BLL.Services.Clasess;
using Graduate.BLL.Services.Interfaces;
using Graduate.DAL.Dto.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Graduation_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

       
        [HttpPost("get-by-major")]
        public async Task<IActionResult> GetCoursesByMajor([FromBody] CoursesRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var majorData = await _courseService.GetCoursesByMajorAsync(request.Major, userId);
            if (majorData == null)
                return NotFound("التخصص غير موجود");

            return Ok(majorData);
        }

       
        [HttpPost("save-completed")]
        public async Task<IActionResult> SaveCompletedCourses([FromBody] SaveCompletedDto request)
        {
            var userId= User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            request.UserId = userId;
            await _courseService.SaveCompletedCoursesAsync(request);
            return Ok("تم حفظ المواد المنجزة");
        }

       
        [HttpGet("completed/{major}")]
        public async Task<IActionResult> GetCompletedCourses(string major)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            var completed = await _courseService.GetCompletedCourseNamesAsync(userId, major);
            return Ok(completed);
        }
    }
}
