using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sep2026.QuizConsoleAppDapper;

QuestionService questionService = new QuestionService();
CategoryService categoryService = new CategoryService();

Start:
Console.WriteLine("\nQuiz App (Dapper)");
Console.WriteLine("-----------------");
Console.WriteLine("Menu : ");
Console.WriteLine("1. Manage questions");
Console.WriteLine("2. Manage categories");
Console.WriteLine("3. Answer questions");
Console.WriteLine("4. Exit");

int option = ReadNumber("Choose an option >>> ");

switch (option)
{
    case 1:
        Console.WriteLine("\nQuestion Management Menu");
        Console.WriteLine("1. View all questions");
        Console.WriteLine("2. Create a question");
        Console.WriteLine("3. Update a question");
        Console.WriteLine("4. Delete a question");
        Console.WriteLine("5. Back to Menu");

        int opt = ReadNumber("Choose an Option >>> ");

        switch (opt)
        {
            case 1:
                questionService.ViewAllQuestions();
                break;

            case 2:
                var categories = categoryService.GetAllCategories();
                if (categories.Count == 0)
                {
                    Console.WriteLine("\nNo categories exist. Please create a category first (Menu option 2)!");
                    break;
                }

                Console.WriteLine("\nAvailable Categories:");
                foreach (var c in categories)
                {
                    Console.WriteLine($"ID [{c.Id}] - {c.CategoryName}");
                }

                int categoryId = ReadNumber("Enter Category ID >>> ");
                string inputQuestionStatement = ReadText("Enter Question Statement >>> ");
                string inputAnswers = ReadText("Enter Question's answers (comma separated) >>> ");
                string[] answers = inputAnswers.Split(',');

                List<string> cleanAnswers = answers
                    .Select(a => a.Trim())
                    .Where(a => !string.IsNullOrEmpty(a))
                    .ToList();

                if (cleanAnswers.Count < 2)
                {
                    Console.WriteLine("Must provide at least 2 answers.");
                    break;
                }

                Console.WriteLine("\nEntered Answers:");
                for (int i = 0; i < cleanAnswers.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {cleanAnswers[i]}");
                }

                int correctChoice = ReadNumber("Choose correct answer number >>> ");
                if (correctChoice < 1 || correctChoice > cleanAnswers.Count)
                {
                    Console.WriteLine("Invalid answer number.");
                    break;
                }

                questionService.CreateQuestion(inputQuestionStatement, categoryId, cleanAnswers, correctChoice - 1);
                break;

            case 3:
                int updateId = ReadNumber("Enter Question ID to update >>> ");
                var existingQuestion = questionService.GetQuestionById(updateId);
                if (existingQuestion == null)
                {
                    Console.WriteLine($"Question with ID {updateId} not found.");
                    break;
                }

                string updateStatement = ReadText("Enter new Question Statement (or press Enter to keep current) >>> ");
                string updateAnswersInput = ReadText("Enter new Answers comma separated (or leave blank to keep) >>> ");

                List<string>? updateCleanAnswers = null;
                int? updateCorrectIndex = null;

                if (!string.IsNullOrWhiteSpace(updateAnswersInput))
                {
                    updateCleanAnswers = updateAnswersInput
                        .Split(',')
                        .Select(a => a.Trim())
                        .Where(a => !string.IsNullOrEmpty(a))
                        .ToList();

                    if (updateCleanAnswers.Count >= 2)
                    {
                        for (int i = 0; i < updateCleanAnswers.Count; i++)
                        {
                            Console.WriteLine($"{i + 1}. {updateCleanAnswers[i]}");
                        }
                        int choice = ReadNumber("Choose correct answer number >>> ");
                        if (choice >= 1 && choice <= updateCleanAnswers.Count)
                        {
                            updateCorrectIndex = choice - 1;
                        }
                    }
                }

                questionService.UpdateQuestion(updateId, updateStatement, null, updateCleanAnswers, updateCorrectIndex);
                break;

            case 4:
                int deleteId = ReadNumber("Enter Question ID to delete >>> ");
                Console.WriteLine("1. Soft Delete (mark as deleted)");
                Console.WriteLine("2. Hard Delete (permanently delete from database)");
                int delType = ReadNumber("Choose delete type >>> ");
                questionService.DeleteQuestion(deleteId, delType == 2);
                break;

            case 5:
                goto Start;
        }
        break;

    case 2:
        Console.WriteLine("\nCategory Management Menu");
        Console.WriteLine("1. View all categories");
        Console.WriteLine("2. Create a category");
        Console.WriteLine("3. Update a category");
        Console.WriteLine("4. Delete a category");
        Console.WriteLine("5. Back to Menu");

        int catOpt = ReadNumber("Choose an Option >>> ");

        switch (catOpt)
        {
            case 1:
                categoryService.ViewAllCategories();
                break;

            case 2:
                string catName = ReadText("Enter Category Name >>> ");
                categoryService.CreateCategory(catName);
                break;

            case 3:
                int catUpdateId = ReadNumber("Enter Category ID to update >>> ");
                string newCatName = ReadText("Enter new Category Name >>> ");
                categoryService.UpdateCategory(catUpdateId, newCatName);
                break;

            case 4:
                int catDeleteId = ReadNumber("Enter Category ID to delete >>> ");
                Console.WriteLine("1. Soft Delete (mark as deleted)");
                Console.WriteLine("2. Hard Delete (permanently delete from database)");
                int catDelType = ReadNumber("Choose delete type >>> ");
                categoryService.DeleteCategory(catDeleteId, catDelType == 2);
                break;

            case 5:
                goto Start;
        }
        break;

    case 3:
        Console.WriteLine("\n--- Answer Question ---");
        var allQuestions = questionService.GetAllQuestions();
        if (allQuestions.Count == 0)
        {
            Console.WriteLine("There are no questions in the list. Please try to create a question first!");
            break;
        }

        List<Question> shuffledList = questionService.Shuffle(allQuestions);
        int score = 0;
        int questionNumber = 1;

        foreach (Question q in shuffledList)
        {
            Console.WriteLine($"\nNo.{questionNumber} Question: {q.Statement}");
            questionNumber++;

            for (int i = 0; i < q.Answers.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {q.Answers[i].Statement}");
            }

            int ansIndex = ReadNumber("Choose answer number >>> ");

            if (ansIndex >= 1 && ansIndex <= q.Answers.Count && q.Answers[ansIndex - 1].IsCorrectAnswer)
            {
                Console.WriteLine("Correct!");
                score++;
            }
            else
            {
                var correct = q.Answers.FirstOrDefault(a => a.IsCorrectAnswer);
                Console.WriteLine($"Incorrect! Correct answer was: {correct?.Statement}");
            }
        }

        questionService.CalculateMark(score, shuffledList.Count);
        break;

    case 4:
        goto End;

    default:
        Console.WriteLine("Invalid option, try again.");
        break;
}
goto Start;

End:
Console.WriteLine("Exiting program .....");


// Helper functions (matching original QuizConsoleApp)

static string ReadText(string message)
{
    while (true)
    {
        Console.Write(message);
        string? input = Console.ReadLine();
        if (input == null)
        {
            throw new EndOfStreamException();
        }
        if (!string.IsNullOrWhiteSpace(input))
        {
            return input.Trim();
        }
        Console.WriteLine("Please enter a value.");
    }
}

static int ReadNumber(string message)
{
    while (true)
    {
        string input = ReadText(message);
        bool isNumber = int.TryParse(input, out int number);
        if (isNumber)
        {
            return number;
        }
        Console.WriteLine("Please enter a whole number.");
    }
}
