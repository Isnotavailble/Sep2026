namespace Sep2026.QuizConsoleAppWebApi.Models
{
    public class AnswerUpdateRequest
    {
        public int Id { get; set; }
        public string? Statement { get; set; }
        public bool? IsCorrect { get; set; }

    }
}