using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Sep2026.QuizConsoleApp.Database.AppDbContextModels;
using Sep2026.QuizConsoleAppWebApi.Models;

namespace Sep2026.QuizConsoleAppWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private AppDbContext _db;
        public QuizController()
        {
            _db = new AppDbContext();
        }
        // view all questions

        [HttpGet]
        public IActionResult GetAllQuestions() {
            var questions = _db.Questions
                .Where(a => a.IsDeleted == false)
                .Include(a => a.Answers)
                .ToList();

            var response = questions.Select(q => ConvertToResponse(q)).ToList();

            return Ok(response);
        }

        //get by id

        [HttpGet("{id}")]
        public IActionResult GetQuestion(int id)
        {
            var question = _db.Questions
                .Include(q => q.Answers)
                .FirstOrDefault(x => x.Id == id);
            
            if (question == null) 
                return NotFound($"Question with Id {id} doesn't exsit.");

            return Ok(ConvertToResponse(question));
        }


        //create question
        [HttpPost]
        public IActionResult CreateQuestion(QuestionCreateRequest request)
        {

            if (request == null)
                return BadRequest("Should define a proper question");
            if (request.Statement == null)
                return BadRequest("Question statement must be defined");

            Category? validCategoryId = _db.Categories.FirstOrDefault(x => x.Id == request.CategoryId);

            if (validCategoryId == null)
                return BadRequest("Invalid Question category Id");

            if (request.AnswersList == null || request.AnswersList.Count < 2)
                return BadRequest("Must define Answers at least 2");

            var validAnswers = request.AnswersList.Where( ans => !string.IsNullOrEmpty(ans.Statement)).ToList();


            List<Answer> answerList = validAnswers
                .Select(ans => 
                new Answer {
                    Statement = ans.Statement!.Trim(),
                    IsCorrectAnswer = ans.IsCorrect,
                    IsDeleted = false
                
                }
                ).ToList();

            Question questionEntity = new Question
            {
                CategoryId = validCategoryId.Id,
                Answers = answerList,
                Statement = request.Statement,
                IsDeleted = false

            };

            _db.Questions.Add(questionEntity);

            int effectedRow = _db.SaveChanges();

            if (effectedRow < 1)
                return StatusCode(StatusCodes.Status500InternalServerError, "Fail to create the question.");

            return Ok(ConvertToResponse(questionEntity));

        }

        //[HttpPatch("{id}")]
        //public IActionResult UpdateQuestoins(int id, QuestionUpdateRequest updateRequest) {
            
            
        //    var targetQuestion = _db.Questions.Include(t => t.Answers).FirstOrDefault(x => x.Id == id);

        //    var validCategory = _db.Categories.Where(c => c.Id == updateRequest.CategoryId).FirstOrDefault();
            
        //    if (targetQuestion == null)
        //        return NotFound($"Question with Id {id} doesn't exsit");

            
        //    if (!string.IsNullOrEmpty(updateRequest.Statement)) {
        //        targetQuestion.Statement = updateRequest.Statement.Trim();
        //    }

        //    if (validCategory != null)
        //    {
        //        targetQuestion.CategoryId = validCategory.Id;
        //    }

        //    if (updateRequest?.AnswersList?.Count > 0)
        //    {
        //        var updateAnswersIds = updateRequest.AnswersList.Select(a => a.Id ).ToList();
        //        var updateAnsersids = 
        //    }


        //    return Ok();
        //}

        
        [HttpDelete]
        public IActionResult DeletQuestionById([FromQuery]int? id , [FromQuery] bool isHardDelete)
        {
            var targetQuestion = _db.Questions.Include(x => x.Answers).FirstOrDefault(x => x.Id == id);
            
            if (targetQuestion == null)
                return NotFound($"Question with Id {id} doesn't exist.");

            if (isHardDelete == true)
            {
                _db.Answers.RemoveRange(targetQuestion.Answers);
                _db.Questions.Remove(targetQuestion);
            }
            else
            {
                targetQuestion.IsDeleted = true;
            }
            
            int status = _db.SaveChanges();
            return status > 0 ? Ok("Successfully deleted") : StatusCode(StatusCodes.Status500InternalServerError, "Couldn't delete");

        }


        //Entity to Dto converter
        private QuestionResponse ConvertToResponse(Question q)
        {
            var response = new QuestionResponse
            {
                Id = q.Id,
                Statement = q.Statement,
                AnswersList = q.Answers.Select(a => new AnswerResponse { Id = a.Id, IsCorrect = a.IsCorrectAnswer, Statement = a.Statement }).ToList()

            };
            return response;
        }
    }
    
}
