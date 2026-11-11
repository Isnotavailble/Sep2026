using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.EntityFrameworkCore;
using Sep2026.GameShopWebApi.Models;
using Sep2026.GameShopWebApi.Utils;
using Sep2026.QuizConsoleApp.Database.GameShopAppDbContext;

namespace Sep2026.GameShopWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsoleCategoriesController : ControllerBase
    {
        private GameShopAppDbContext _db;
        private ModelConverter _modelConverter;
        public ConsoleCategoriesController()
        {
            _db = new GameShopAppDbContext();
            _modelConverter = new ModelConverter();
        }

        [HttpGet]
        public IActionResult GetAllConsoleCategories()
        {
            var response = _db.ConsoleCategories.ToList().Select(_modelConverter.ConvertConsoleCategoryResponse);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public IActionResult GetConsoleCategoryById(int id)
        {
            var category = _db.ConsoleCategories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return Ok(_modelConverter.ConvertConsoleCategoryResponse(category));
        }

        [HttpPost]
        public IActionResult CreateConsoleCategory(ConsoleCategoryModel.CreateRequest createRequest)
        {
            if (createRequest == null) { return BadRequest("Must define a body"); }

            if (string.IsNullOrEmpty(createRequest.Name)) { return BadRequest("Category's name must be defined"); }

            if (createRequest.HourlyPrice < 5000m) { return BadRequest("Hourly price should have minimum 5000mmk"); }

            var newCategory = new ConsoleCategory
            {
                HourlyRate = createRequest.HourlyPrice,
                Name = createRequest.Name,
                IsDeleted = false,
            };

            _db.ConsoleCategories.Add(newCategory);
            int status = _db.SaveChanges();

            return status > 0 ? Ok(newCategory) : BadRequest("Fail to create new console category");

        }

        [HttpPatch("{id}")]
        public IActionResult UpdateCategory(int id, ConsoleCategoryModel.UpdateRequest updateRequest)
        {
            
            var targetCategory = _db.ConsoleCategories.FirstOrDefault(c => c.Id == id);

            if (targetCategory == null) { return NotFound("Console category with this Id doesn't exsit"); }

            if (!string.IsNullOrEmpty(updateRequest.Name)) {
                targetCategory.Name = updateRequest.Name;
            }

            if (updateRequest.HourlyPrice.HasValue && updateRequest.HourlyPrice.Value > 5000m) {
                
                targetCategory.HourlyRate = updateRequest.HourlyPrice.Value;                
            }

            int status = _db.SaveChanges();
            return status > 0 ? Ok(_modelConverter.ConvertConsoleCategoryResponse(targetCategory)) : BadRequest("Fail to update.");

        }
        //delete
        [HttpDelete("{id}")]
        public IActionResult DeleteCategory(int id, [FromQuery] bool isHardDelete = false)
        {
            var targetCategory = _db.ConsoleCategories.FirstOrDefault(c => c.Id == id);
            if (targetCategory == null) return NotFound("Category with this Id doesn't exsit");
            //hard delete
            if (isHardDelete == true)
            {
                //count child entity first to avoid fk or unwanted cascade delete.

                int consolesCount = _db.GameConsoles.Count(g => g.CategoryId == targetCategory.Id);
                int gamesCount = targetCategory.Games.Count(g => g.CategoryId == targetCategory.Id);

                if (consolesCount > 0 || gamesCount > 0)
                {
                    return BadRequest($"Category {targetCategory.Name} having 1 or more consoles and games.Only soft-delete is allowed.");
                }

                _db.ConsoleCategories.Remove(targetCategory);
                
            }
            //soft delete
            else
            {
                targetCategory.IsDeleted = true;

            }

            int status = _db.SaveChanges();
            return status > 0 ? Ok("Successfully deleted") : BadRequest("Fail to delete");

        }
    }
}
