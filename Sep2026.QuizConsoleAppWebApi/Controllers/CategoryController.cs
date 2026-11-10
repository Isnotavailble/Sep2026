using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Sep2026.QuizConsoleApp.Database.AppDbContextModels;
using Sep2026.QuizConsoleAppWebApi.Models.CategoryModels;

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
        public IActionResult GetAllCategories()
        {
            var categories = _db.Categories.ToList();
            var response = categories.Select(ConvertToResponse).ToList();
            return Ok(response);

        }

        [HttpGet("{id}")]
        public IActionResult GetCategoryById(int id)
        {
            var targetCategory = _db.Categories.FirstOrDefault(x => x.Id == id);
            if (targetCategory == null)
                return NotFound($"No category with Id {id}");

            return Ok(ConvertToResponse(targetCategory));
        }

        [HttpPost]
        public IActionResult CreateCategory(CategoryCreateRequest categoryCreateRequest)
        {
            if (string.IsNullOrEmpty(categoryCreateRequest?.CategoryName))
                return BadRequest("Must define a category name");

            Category category = new Category
            {
                CategoryName = categoryCreateRequest.CategoryName,
                IsDeleted = false
            };

            _db.Categories.Add(category);
            int status = _db.SaveChanges();

            if (status < 1)
            {
                return BadRequest("Couldn't create a new category");
            }

            return Ok(ConvertToResponse(category));
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCategory(int id,CategoryUpdateRequest categoryUpdateRequest)
        {
            var targetCategory = _db.Categories.FirstOrDefault(c => c.Id == id);
            if (targetCategory == null)
                return NotFound($"No category with Id {id}");

            targetCategory.CategoryName = string.IsNullOrEmpty(categoryUpdateRequest.CategoryName) ? targetCategory.CategoryName : categoryUpdateRequest.CategoryName;
            _db.SaveChanges();
            return Ok(ConvertToResponse(targetCategory));
        }

        [HttpDelete]
        public IActionResult DeleteCategory([FromQuery] int id , [FromQuery] bool isHardDelete)
        {
            var targetCategory = _db.Categories.FirstOrDefault(c => c.Id == id);

            if (targetCategory == null)
                return NotFound($"Category with ID {id} doesn't exist");

            int questionByCategory = _db.Questions.Where(q => q.CategoryId == targetCategory.Id).Count();

            //block hard delete when having child entities.
            if (isHardDelete == true && questionByCategory > 0)
                return BadRequest($"Only soft delete is allowed for category having {questionByCategory} questions.Try removing the quesitons for this category");

            else if (isHardDelete == true)
            {
                _db.Categories.Remove(targetCategory);
            }
            else if(isHardDelete == false )
            {
                targetCategory.IsDeleted = true;
            }

            int status = _db.SaveChanges();
            return status > 0? Ok("Successfully deleted") : BadRequest($"Couldn't delete the category with Id {id}");

        }
        private CategoryResponse ConvertToResponse(Category category)
        {
            return new CategoryResponse
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                IsDeleted = category.IsDeleted,
            };
        }
        

    }
}
