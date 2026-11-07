using System;
using System.Collections.Generic;

namespace Sep2026.QuizConsoleApp.Database.AppDbContextModels;

public partial class Question
{
    public int Id { get; set; }

    public string Statement { get; set; } = null!;

    public int CategoryId { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();

    public virtual Category Category { get; set; } = null!;
}
