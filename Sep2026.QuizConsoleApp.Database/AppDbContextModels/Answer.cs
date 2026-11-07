using System;
using System.Collections.Generic;

namespace Sep2026.QuizConsoleApp.Database.AppDbContextModels;

public partial class Answer
{
    public int Id { get; set; }

    public int QuestionId { get; set; }

    public string Statement { get; set; } = null!;

    public bool IsCorrectAnswer { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Question Question { get; set; } = null!;
}
