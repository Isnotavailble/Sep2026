using System.Data;

namespace Sep2026.QuizConsoleAppWebApi.Models
{
    public class QuestionCreateRequest
    {
        public string ? Statement { get; set; }
        public int CategoryId { get; set; }
        public List<AnswerCreateRequest>? AnswersList { get; set; }
    }
}
