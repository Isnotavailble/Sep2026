using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            if (category == null) {
                return NotFound();
            }
            return Ok(_modelConverter.ConvertConsoleCategoryResponse(category));
        }

    }
}
