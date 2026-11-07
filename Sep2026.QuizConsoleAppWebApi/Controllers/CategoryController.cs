using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sep2026.QuizConsoleApp.Database.AppDbContextModels;

namespace Sep2026.QuizConsoleAppWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private AppDbContext _db;
        public CategoryController() {
            _db = new AppDbContext();
        
        }

        [HttpGet]
        public IActionResult CreateMockCategory()
        {
            Category category1 = new Category
            {
                CategoryName = "coding",
                IsDeleted = false

            };
            Category category2 = new Category
            {
                CategoryName = "math",
                IsDeleted = false

            };
            Category category3 = new Category
            {
                CategoryName = "normal",
                IsDeleted = false

            };
            int totalEntities = _db.Categories.Count();
            if (totalEntities > 0)
                return BadRequest("Category Mock is already created");
            _db.Categories.Add(category1);
            _db.Categories.Add(category2);
            _db.Categories.Add(category3);
            int status = _db.SaveChanges();
            if (status < 1)
                return StatusCode(StatusCodes.Status500InternalServerError, "Couldn't insert the categories");
            List<Category> response = [category1, category2, category3];

            return Ok(response);

        }
    }
}
