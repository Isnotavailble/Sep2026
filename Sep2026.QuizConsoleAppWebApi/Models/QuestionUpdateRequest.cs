using System.Data;

namespace Sep2026.QuizConsoleAppWebApi.Models
{
    public class QuestionUpdateRequest
    {
        public string? Statement { get; set; }
        public int? CategoryId { get; set; }
        public List<AnswerUpdateRequest>? AnswersList { get; set; }
    }
}
