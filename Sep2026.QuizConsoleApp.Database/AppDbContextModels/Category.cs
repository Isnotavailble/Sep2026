using System;
using System.Collections.Generic;

namespace Sep2026.QuizConsoleApp.Database.AppDbContextModels;

public partial class Category
{
    public int Id { get; set; }

    public string CategoryName { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}
