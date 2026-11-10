namespace Sep2026.QuizConsoleAppWebApi.Models
{
    public class QuestionResponse
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string ? Statement { get; set; }
        public List<AnswerResponse> ? AnswersList { get; set; }

    }
}
