namespace Sep2026.QuizConsoleAppWebApi.Models.CategoryModels
{
    public class CategoryResponse
    {
        public int Id { get; set; }
        public string? CategoryName { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
    }
}
