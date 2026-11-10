using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Sep2026.QuizConsoleAppDapper
{
    public class Question
    {
        public int Id { get; set; }
        public string Statement { get; set; } = "";
        public int CategoryId { get; set; }
        public bool IsDeleted { get; set; }
        public List<Answer> Answers { get; set; } = new List<Answer>();
    }

    public class Answer
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string Statement { get; set; } = "";
        public bool IsCorrectAnswer { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class QuestionService
    {
        private SqlConnectionStringBuilder _connection = new SqlConnectionStringBuilder
        {
            UserID = "sa",
            Password = "sasa@123",
            DataSource = ".",
            InitialCatalog = "QuizDb",
            TrustServerCertificate = true
        };

        public List<Question> GetAllQuestions()
        {
            IDbConnection connection = new SqlConnection(_connection.ToString());
            connection.Open();

            string questionSql = "SELECT * FROM Questions WHERE IsDeleted = 0";
            var questions = connection.Query<Question>(questionSql).ToList();

            string answerSql = "SELECT * FROM Answers WHERE IsDeleted = 0";
            var answers = connection.Query<Answer>(answerSql).ToList();

            connection.Close();

            foreach (var q in questions)
            {
                q.Answers = answers.Where(a => a.QuestionId == q.Id).ToList();
            }

            return questions;
        }

        public Question? GetQuestionById(int id)
        {
            IDbConnection connection = new SqlConnection(_connection.ToString());
            connection.Open();

            string questionSql = "SELECT * FROM Questions WHERE Id = @Id AND IsDeleted = 0";
            var question = connection.Query<Question>(questionSql, new { Id = id }).FirstOrDefault();

            if (question != null)
            {
                string answerSql = "SELECT * FROM Answers WHERE QuestionId = @QuestionId AND IsDeleted = 0";
                question.Answers = connection.Query<Answer>(answerSql, new { QuestionId = id }).ToList();
            }

            connection.Close();

            return question;
        }

        public void ViewAllQuestions()
        {
            var questions = GetAllQuestions();
            if (questions.Count == 0)
            {
                Console.WriteLine("\nThere are no questions in the list. Please create some questions first.");
                return;
            }

            foreach (var q in questions)
            {
                Console.WriteLine($"\nID ({q.Id}) Question: {q.Statement}");
                for (int i = 0; i < q.Answers.Count; i++)
                {
                    string mark = q.Answers[i].IsCorrectAnswer ? " (Correct)" : "";
                    Console.WriteLine($"{i + 1}. {q.Answers[i].Statement}{mark}");
                }
            }
        }

        public void CreateQuestion(string statement, int categoryId, List<string> answers, int correctAnswerIndex)
        {
            if (string.IsNullOrWhiteSpace(statement))
            {
                Console.WriteLine("Question statement cannot be empty.");
                return;
            }

            if (answers == null || answers.Count < 2)
            {
                Console.WriteLine("Must provide at least 2 answers.");
                return;
            }

            if (correctAnswerIndex < 0 || correctAnswerIndex >= answers.Count)
            {
                Console.WriteLine("Invalid correct answer selection.");
                return;
            }

            IDbConnection connection = new SqlConnection(_connection.ToString());
            connection.Open();

            // Verify Category exists
            string checkCategorySql = "SELECT COUNT(*) FROM Category WHERE Id = @CategoryId AND IsDeleted = 0";
            int categoryExists = connection.ExecuteScalar<int>(checkCategorySql, new { CategoryId = categoryId });
            if (categoryExists == 0)
            {
                Console.WriteLine($"Invalid Category ID: {categoryId}");
                connection.Close();
                return;
            }

            // Insert Question and retrieve new ID
            string insertQuestionSql = "INSERT INTO Questions (Statement, CategoryId, IsDeleted) VALUES (@Statement, @CategoryId, 0); SELECT CAST(SCOPE_IDENTITY() as int);";
            int questionId = connection.ExecuteScalar<int>(insertQuestionSql, new { Statement = statement.Trim(), CategoryId = categoryId });

            // Insert Answers
            for (int i = 0; i < answers.Count; i++)
            {
                string insertAnswerSql = "INSERT INTO Answers (QuestionId, Statement, IsCorrectAnswer, IsDeleted) VALUES (@QuestionId, @Statement, @IsCorrectAnswer, 0)";
                connection.Execute(insertAnswerSql, new
                {
                    QuestionId = questionId,
                    Statement = answers[i].Trim(),
                    IsCorrectAnswer = (i == correctAnswerIndex)
                });
            }

            connection.Close();

            Console.WriteLine("Question created successfully.");
        }

        public void UpdateQuestion(int id, string? newStatement, int? newCategoryId, List<string>? newAnswers, int? newCorrectAnswerIndex)
        {
            var question = GetQuestionById(id);
            if (question == null)
            {
                Console.WriteLine($"Question with ID {id} not found.");
                return;
            }

            IDbConnection connection = new SqlConnection(_connection.ToString());
            connection.Open();

            // Update Statement
            if (!string.IsNullOrWhiteSpace(newStatement))
            {
                string updateStatementSql = "UPDATE Questions SET Statement = @Statement WHERE Id = @Id";
                connection.Execute(updateStatementSql, new { Statement = newStatement.Trim(), Id = id });
            }

            // Update Category
            if (newCategoryId.HasValue && newCategoryId.Value > 0)
            {
                string checkCategorySql = "SELECT COUNT(*) FROM Category WHERE Id = @CategoryId AND IsDeleted = 0";
                int categoryExists = connection.ExecuteScalar<int>(checkCategorySql, new { CategoryId = newCategoryId.Value });
                if (categoryExists > 0)
                {
                    string updateCatSql = "UPDATE Questions SET CategoryId = @CategoryId WHERE Id = @Id";
                    connection.Execute(updateCatSql, new { CategoryId = newCategoryId.Value, Id = id });
                }
            }

            // Update Answers if provided
            if (newAnswers != null && newAnswers.Count >= 2 && newCorrectAnswerIndex.HasValue)
            {
                // Delete old answers
                string deleteAnswersSql = "DELETE FROM Answers WHERE QuestionId = @QuestionId";
                connection.Execute(deleteAnswersSql, new { QuestionId = id });

                // Insert updated answers
                for (int i = 0; i < newAnswers.Count; i++)
                {
                    string insertAnswerSql = "INSERT INTO Answers (QuestionId, Statement, IsCorrectAnswer, IsDeleted) VALUES (@QuestionId, @Statement, @IsCorrectAnswer, 0)";
                    connection.Execute(insertAnswerSql, new
                    {
                        QuestionId = id,
                        Statement = newAnswers[i].Trim(),
                        IsCorrectAnswer = (i == newCorrectAnswerIndex.Value)
                    });
                }
            }

            connection.Close();

            Console.WriteLine("Question updated successfully.");
        }

        public void DeleteQuestion(int id, bool isHardDelete)
        {
            var question = GetQuestionById(id);
            if (question == null)
            {
                Console.WriteLine($"Question with ID {id} not found.");
                return;
            }

            IDbConnection connection = new SqlConnection(_connection.ToString());
            connection.Open();

            if (isHardDelete)
            {
                connection.Execute("DELETE FROM Answers WHERE QuestionId = @Id", new { Id = id });
                connection.Execute("DELETE FROM Questions WHERE Id = @Id", new { Id = id });
            }
            else
            {
                connection.Execute("UPDATE Answers SET IsDeleted = 1 WHERE QuestionId = @Id", new { Id = id });
                connection.Execute("UPDATE Questions SET IsDeleted = 1 WHERE Id = @Id", new { Id = id });
            }

            connection.Close();

            Console.WriteLine($"Question with ID {id} has been deleted successfully.");
        }

        public List<Question> Shuffle(List<Question> questions)
        {
            List<Question> result = new List<Question>(questions);
            for (int i = result.Count - 1; i > 0; i--)
            {
                int j = Random.Shared.Next(i + 1);
                var temp = result[i];
                result[i] = result[j];
                result[j] = temp;
            }
            return result;
        }

        public void CalculateMark(int correctCount, int totalQuestions)
        {
            if (totalQuestions == 0) return;
            double correctRate = Math.Round(((double)correctCount / totalQuestions) * 100, 2);
            Console.WriteLine($"\nCorrect {correctRate}% ( {correctCount} out of {totalQuestions} questions )");
        }
    }
}
