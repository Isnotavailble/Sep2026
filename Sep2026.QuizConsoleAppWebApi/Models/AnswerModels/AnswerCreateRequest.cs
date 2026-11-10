namespace Sep2026.QuizConsoleAppWebApi.Models.AnswerModels
{
    public class AnswerCreateRequest
    {
        public string ? Statement { get; set; }
        public bool IsCorrect { get; set; }
    }
}
