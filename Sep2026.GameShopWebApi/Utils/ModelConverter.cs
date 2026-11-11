using Sep2026.GameShopWebApi.Models;
using Sep2026.QuizConsoleApp.Database.GameShopAppDbContext;

namespace Sep2026.GameShopWebApi.Utils
{
    public class ModelConverter
    {
        public  GameConsoleModel.Response ConvertGameConsoleResponse(GameConsole gameConsole)
        {
            return new GameConsoleModel.Response { 
                Id = gameConsole.Id,
                ConsoleNumber = gameConsole.ConsoleNumber,
                CategoryId = gameConsole.CategoryId,
                CurrentStatus = gameConsole.CurrentStatus,
                IsDeleted = gameConsole.IsDeleted,
            };
        }
        public ConsoleCategoryModel.Response ConvertConsoleCategoryResponse(ConsoleCategory category)
        {
            return new ConsoleCategoryModel.Response
            {
                Id = category.Id,
                Name = category.Name,
                HourlyPrice = category.HourlyRate,
                IsDeleted = category.IsDeleted,
            };
        }
    }
}
