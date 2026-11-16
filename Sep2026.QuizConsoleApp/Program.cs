using System;
using System.Collections.Generic;
using System.Text.Json;
using QuizConsoleApp;

QuestionService questionService = new QuestionService();

Start:
Console.WriteLine("\nQuiz App");
Console.WriteLine("-------------");
Console.WriteLine("Menu : ");
Console.WriteLine("1. Manage questions");
Console.WriteLine("2. Answer questions");
Console.WriteLine("3. Exit");

// Using ReadNumber so it doesn't crash on letters or invalid inputs
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
                string inputQuestionStatement = ReadText("Enter Question Statement >>> ");
                string inputAnswers = ReadText("Enter Question's answers (comma separated) >>> ");
                string[] answers = inputAnswers.Split(',');
                string inputCorrectAnswer = ReadText("Enter correct answer >>> ");

                QuestionDTO questionDTO = new QuestionDTO
                {
                    QuestionStatement = inputQuestionStatement.Trim(),
                    Answers = answers,
                    CorrectAnswer = inputCorrectAnswer.Trim()
                };
                questionService.CreateQuestion(questionDTO);
                break;

            case 3:
                if (questionService.QuestionDTOList.Count == 0)
                {
                    Console.WriteLine("\nThere are no questions in the list. Please try to create a question first!");
                    break;
                }

                int updateId = ReadNumber("Enter Question ID to update >>> ");
                string updateStatement = ReadText("Enter new Question Statement >>> ");
                string updateAnswersInput = ReadText("Enter new Answers (comma separated) >>> ");
                string[] updateAnswers = updateAnswersInput.Split(',');
                string updateCorrect = ReadText("Enter new correct answer >>> ");

                QuestionDTO updateDTO = new QuestionDTO
                {
                    Id = updateId,
                    QuestionStatement = updateStatement,
                    Answers = updateAnswers,
                    CorrectAnswer = updateCorrect
                };
                questionService.UpdateQuestion(updateDTO);
                break;

            case 4:
                if (questionService.QuestionDTOList.Count == 0)
                {
                    Console.WriteLine("\nThere are no questions in the list. Please try to create a question first!");
                    break;
                }
                int deleteId = ReadNumber("Enter Question ID to delete >>> ");
                questionService.DeleteQuestion(deleteId);
                break;

            case 5:
                goto Start;
        }
        break;

    case 2:
        Console.WriteLine("\n--- Answer Question ---");

        if (questionService.QuestionDTOList.Count == 0)
        {
            Console.WriteLine("There are no questions in the list. Please try to create a question first!");
            break;
        }

        List<QuestionDTO> shuffledList = questionService.Shuffle();
        int score = 0;
        int questionNumber = 1;

        foreach (QuestionDTO q in shuffledList)
        {
            Console.WriteLine($"\nNo.{questionNumber} Question: {q.QuestionStatement}");
            questionNumber++;

            for (int i = 0; i < q.Answers.Length; i++)
            {
                int optNum = i + 1;
                Console.WriteLine($"{optNum}. {q.Answers[i].Trim()}");
            }

            int ansIndex = ReadNumber("Choose answer number >>> ");

            // Fixed the typo here (was ansIndex1\n)
            bool isCorrect = questionService.CheckAnswer(q.Id, ansIndex - 1);
            if (isCorrect)
            {
                Console.WriteLine("Correct!");
                score++;
            }
            else
            {
                Console.WriteLine("Incorrect!");
            }
        }

        questionService.CalculateMark(score);
        break;

    case 3:
        goto End;

    default:
        Console.WriteLine("Invalid option, try again.");
        break;
}
goto Start;
End:
//questionService.SaveToJsonFile();
Console.WriteLine("Exiting program .....");


// helper function
JsonSerializer

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
        //out keyword create the var directly on success.
        bool isNumber = int.TryParse(input, out int number);
        if (isNumber)
        {
            return number;
        }
        Console.WriteLine("Please enter a whole number.");
    }
}