using Sep2026.GameShopWebApi.Models;
using Sep2026.QuizConsoleApp.Database.GameShopAppDbContext;

namespace Sep2026.GameShopWebApi.Utils
{
    public class ModelConverter
    {
        public GameConsoleModel.Response ConvertGameConsoleResponse(GameConsole gameConsole)
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

        public GamesModel.Response ConvertGameResponse(Game game)
        {
            return new GamesModel.Response
            {
                Id = game.Id,
                Title = game.Title,
                CategoryId = game.CategoryId,
                TotalStock = game.TotalStock,
                AvailableStock = game.AvailableStock,
                IsDeleted = game.IsDeleted,
            };
        }
    }
}
