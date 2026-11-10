using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;

namespace Sep2026.QuizConsoleAppDapper
{
    public class Category
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = "";
        public bool IsDeleted { get; set; }
    }

    public class CategoryService
    {
        //private string _connectionString = "Server=.;Database=QuizDb;User Id=sa;Password=sasa@123;TrustServerCertificate=True;";
        private SqlConnectionStringBuilder _connection = new SqlConnectionStringBuilder { 
            UserID = "sa",
            Password = "sasa@123",
            DataSource = ".",
            InitialCatalog="QuizDb",
            TrustServerCertificate = true
        };
        

        public List<Category> GetAllCategories()
        {
            IDbConnection connection = new SqlConnection(_connection.ToString());
            connection.Open();
            string sql = "SELECT * FROM Category WHERE IsDeleted = 0";
            var categoryList = connection.Query<Category>(sql);
            connection.Close();

            return categoryList.ToList();
        }

        public Category? GetCategoryById(int id)
        {
            IDbConnection connection = new SqlConnection(_connection.ToString());
            connection.Open();
            string sql = "SELECT * FROM Category WHERE Id = @Id AND IsDeleted = 0";
            var category = connection.Query<Category>(sql, new { Id = id }).FirstOrDefault();
            connection.Close();
            return category;
        }

        public void ViewAllCategories()
        {
            var categories = GetAllCategories();
            if (categories.Count == 0)
            {
                Console.WriteLine("\nNo categories found.");
                return;
            }

            Console.WriteLine("\n--- Categories ---");
            foreach (var cat in categories)
            {
                Console.WriteLine($"ID: {cat.Id} | Name: {cat.CategoryName}");
            }
        }

        public void CreateCategory(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                Console.WriteLine("Category name cannot be empty.");
                return;
            }

            IDbConnection connection = new SqlConnection(_connection.ToString());

            connection.Open();

            string sql = "INSERT INTO Category (CategoryName, IsDeleted) VALUES (@CategoryName, 0)";
            int result = connection.Execute(sql, new { CategoryName = categoryName.Trim() });

            connection.Close();

            if (result > 0)
                Console.WriteLine("Category created successfully.");
            else
                Console.WriteLine("Failed to create category.");
        }

        public void UpdateCategory(int id, string newCategoryName)
        {
            if (string.IsNullOrWhiteSpace(newCategoryName))
            {
                Console.WriteLine("Category name cannot be empty.");
                return;
            }

            IDbConnection connection = new SqlConnection(_connection.ToString());

            connection.Open();

            string sql = "UPDATE Category SET CategoryName = @CategoryName WHERE Id = @Id AND IsDeleted = 0";
            int result = connection.Execute(sql, new { CategoryName = newCategoryName.Trim(), Id = id });

            connection.Close();
            
            if (result > 0)
                Console.WriteLine("Category updated successfully.");
            else
                Console.WriteLine($"Category with ID {id} not found.");
        }

        public void DeleteCategory(int id, bool isHardDelete)
        {
            IDbConnection connection = new SqlConnection(_connection.ToString());
            connection.Open();
           
            string countSql = "SELECT COUNT(*) FROM Questions WHERE CategoryId = @CategoryId AND IsDeleted = 0";
            int questionCount = connection.ExecuteScalar<int>(countSql, new { CategoryId = id });

            if (isHardDelete && questionCount > 0)
            {
                Console.WriteLine($"Cannot hard delete: category has {questionCount} questions. Remove questions first or use soft delete.");
                return;
            }

            string deleteSql = isHardDelete
                ? "DELETE FROM Category WHERE Id = @Id"
                : "UPDATE Category SET IsDeleted = 1 WHERE Id = @Id";

            int result = connection.Execute(deleteSql, new { Id = id });

            connection.Close();

            if (result > 0)
                Console.WriteLine("Category deleted successfully.");
            else
                Console.WriteLine($"Category with ID {id} not found.");
        }
    }
}
