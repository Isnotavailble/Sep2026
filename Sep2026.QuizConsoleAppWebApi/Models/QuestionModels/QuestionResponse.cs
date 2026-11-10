using Sep2026.QuizConsoleAppWebApi.Models.AnswerModels;

namespace Sep2026.QuizConsoleAppWebApi.Models.QuestionModels
{
    public class QuestionResponse
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string ? Statement { get; set; }
        public List<AnswerResponse> ? AnswersList { get; set; }

    }
}
