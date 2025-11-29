using Graduate.DAL.Dto.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Graduate.BLL.Services.Clasess
{
    public class RagService
    {
        private readonly HttpClient _httpClient;

        public RagService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<RagResponse> GetRagResponseAsync(string question, string major, List<string> completedCourses,string year,string semester)
        {
            var payload = new { query = question, major = major ,completedCourses = completedCourses,year,semester };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/ask", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(responseContent);
            using var jsonDoc = JsonDocument.Parse(responseContent);
            var answerElement = jsonDoc.RootElement.GetProperty("answer");
            var result = answerElement.GetProperty("result").GetString();
            var sources = answerElement.GetProperty("sources")
                           .EnumerateArray()
                           .Select(x => x.GetString())
                           .ToList();
            return new RagResponse
            {
                Result = result,
                Sources = sources
            };
        }
    }
}
