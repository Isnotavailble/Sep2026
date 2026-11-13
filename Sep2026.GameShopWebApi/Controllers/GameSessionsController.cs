using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sep2026.GameShopWebApi.Models;
using Sep2026.GameShopWebApi.Utils;
using Sep2026.QuizConsoleApp.Database.GameShopAppDbContext;

namespace Sep2026.GameShopWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameSessionsController : ControllerBase
    {
        private GameShopAppDbContext _db;
        private ModelConverter _modelConverter;
        public GameSessionsController()
        {
            _db = new GameShopAppDbContext();
            _modelConverter = new ModelConverter();
        }
        [HttpGet]
        public IActionResult GetGameSessions()
        {
            var gameSessions = _db.GameSessions.ToList();
            var gameSessionsResponses = gameSessions.Select(_modelConverter.ConvertGameSessionResponse).ToList();
            return Ok(gameSessionsResponses);
        }

        [HttpGet("{id}")]
        public IActionResult GetGameSessionById(int id)
        {
            var gameSession = _db.GameSessions.FirstOrDefault(x => x.Id == id);
            if (gameSession == null)
                return NotFound($"Game session with Id {id} is not found.");

            return Ok(_modelConverter.ConvertGameSessionResponse(gameSession));
        }

        [HttpPost]
        public IActionResult CreateGameSession(GameSessionModel.CreateRequest createRequest)
        {

            if (createRequest == null)
                return BadRequest("Body must be defined");

            if (!createRequest.ConsoleId.HasValue)
                return BadRequest("Must define a seat or game console for this session.");

            //check console
            var reqConsole = _db.GameConsoles
                .Include(g => g.Category)
                .FirstOrDefault(g =>
                    g.Id == createRequest.ConsoleId.Value &&
                    g.CurrentStatus.ToLower() == "available" &&
                    g.IsDeleted == false
                 );

            if (reqConsole == null)
                return BadRequest($"Console with Id {createRequest.ConsoleId.Value} is currently unavailabe.");

            //check time interval
            if (!createRequest.StartTime.HasValue || !createRequest.EndTime.HasValue)
                return BadRequest("Time interval must be defined.");

            if (createRequest.StartTime.Value > createRequest.EndTime.Value)
                return BadRequest("EndTime should not come first.");


            //check game ( allowed null )
            // note : this field can be null.later owner can add this.
            if (createRequest.GameId.HasValue && createRequest.GameId <= 0)
            {
                var reqGame = _db.Games.FirstOrDefault(game => game.Id == createRequest.GameId && game.IsDeleted == false);
                if (reqGame == null)
                    return NotFound("Game is not found.");

            }

            //check booking if user provide bookingId value ( allowed null )
            //note : owner doesn't care about late, the endTime will be the same as booked endTme.
            bool isBooking = false;
            decimal totalAmount = 0;
            if (createRequest.BookingId.HasValue && createRequest.BookingId.Value > 0)
            {

                //check booking
                var reqBooking = _db.Bookings
                    .FirstOrDefault(booking =>
                        booking.Id == createRequest.BookingId &&
                        booking.BookingStatus.ToLower() != "cancelled" &&
                        booking.BookingStatus.ToLower() != "completed" &&
                        booking.IsDeleted == false
                        );
                if (reqBooking == null)
                    return NotFound($"Booking with Id {createRequest.BookingId} doesn't cancalled or completed");

                if (createRequest.ConsoleId != reqBooking.ConsoleId)
                    return BadRequest($"Booking with Id {createRequest.BookingId} didn't register game console ( {reqConsole.ConsoleNumber} )");

                //check if costumer arrived earlier than book starttime.Owner will ask them to wait.
                if (createRequest.StartTime.Value < reqBooking.StartTime.AddMinutes(-5))
                    return BadRequest($"Current session startTime is earlier than booked startTime.");

                //check if costumer arrived after the endTime.
                if (createRequest.StartTime.Value >= reqBooking.EndTime)
                    return BadRequest($"This session is  already expire");

                //check totalAmount vs depositAmount
                var totalPaidAmount = createRequest.PaidAmount + reqBooking.DepositAmount;
                if (totalPaidAmount < reqBooking.TotalAmount)
                    return BadRequest($"Insufficent amount.Required amount is {reqBooking.TotalAmount - totalPaidAmount}mmk.");

                if (totalPaidAmount > reqBooking.TotalAmount)
                    return BadRequest($"Exceeded amount {totalPaidAmount - reqBooking.TotalAmount}");


                isBooking = true;
                totalAmount = reqBooking.TotalAmount;
                //update booking if this transaction success.
                reqBooking.BookingStatus = "Completed";
                reqBooking.UpdatedAt = DateTime.Now;

                createRequest.PaidAmount = totalPaidAmount;
                createRequest.EndTime = reqBooking.EndTime;// to make sure the booking endtime is the same. 

            }

            //check time interval for booking and session
            //rule : checking bookings is for walkin customers , checking session is for duplicate detection (double creation of session).
            bool isBookingExist = false;
            if (!isBooking)
            {
                isBookingExist = _db.Bookings
                .Any(booking =>
                    booking.ConsoleId == createRequest.ConsoleId &&
                    booking.StartTime < createRequest.EndTime.Value &&
                    booking.EndTime > createRequest.StartTime.Value &&
                    booking.BookingStatus.ToLower() != "completed" &&
                    booking.BookingStatus.ToLower() != "cancelled" &&
                    booking.IsDeleted == false
                );
            }

            //final check of the booking customer 
            //note: this block can be move to booking check statement but I kept this here for visibility
            else
            {
                isBookingExist = _db.Bookings
               .Any(booking =>
                   booking.Id != createRequest.BookingId &&
                   booking.ConsoleId == createRequest.ConsoleId &&
                   booking.StartTime < createRequest.EndTime.Value &&
                   booking.EndTime > createRequest.StartTime.Value &&
                   booking.BookingStatus.ToLower() != "completed" &&
                   booking.BookingStatus.ToLower() != "cancelled" &&
                   booking.IsDeleted == false
                );

                

            }

            bool isSessionExist = _db.GameSessions
                .Any(session =>
                    session.ConsoleId == createRequest.ConsoleId &&
                    session.StartTime < createRequest.EndTime.Value &&
                    session.EndTime > createRequest.StartTime.Value &&
                    session.IsDeleted == false

                );
            //note : is this seat has session ? , if it is booking, is there overlapbooking ?
            if (isSessionExist || isBookingExist)
                return BadRequest($"Cannot create session due to conflict with bookings or sessions");

            //calculate totalhours and total amount for both type of customer
            TimeSpan sessionDuration = createRequest.EndTime.Value - createRequest.StartTime.Value;
            decimal totalHours = (decimal) sessionDuration.TotalHours;
            
            if (!isBooking)
            {
                totalAmount = reqConsole.Category.HourlyRate * totalHours;
            }
            

            GameSession newGameSession = new GameSession
            {
                BookingId = createRequest.BookingId.HasValue ? createRequest.BookingId.Value : null, 
                ConsoleId = createRequest.ConsoleId.Value,
                GameId = createRequest.GameId.HasValue ? createRequest.GameId.Value : null,
                TotalHours = totalHours,
                TotalAmount = totalAmount,
                PaidAmount = createRequest.PaidAmount,
                StartTime = createRequest.StartTime.Value,
                EndTime = createRequest.EndTime.Value,
                IsDeleted = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,


            };
            
            _db.GameSessions.Add(newGameSession);
            
            int status = _db.SaveChanges();
            return status > 0 ? Ok(_modelConverter.ConvertGameSessionResponse(newGameSession)) : BadRequest("Fail to create session");

        }

    }
}
