using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graduate.DAL.Dto.Response
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public T? Data { get; set; }
        public static ApiResponse<T> SuccessResponse(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

        public static ApiResponse<T> FailResponse(string message)
            => new() { Success = false, Message = message };
    }
}
