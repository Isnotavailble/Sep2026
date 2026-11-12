using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sep2026.GameShopWebApi.Models;
using Sep2026.GameShopWebApi.Utils;
using Microsoft.EntityFrameworkCore;
using Sep2026.QuizConsoleApp.Database.GameShopAppDbContext;

namespace Sep2026.GameShopWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private GameShopAppDbContext _db;
        private ModelConverter _modelConverter;
        public BookingController()
        {
            _db = new GameShopAppDbContext();
            _modelConverter = new ModelConverter();
        }

        //get all booking
        [HttpGet]
        public IActionResult GetAllBookings()
        {
            var bookings = _db.Bookings.ToList().Select(_modelConverter.ConvertBookingResponse);
            return Ok(bookings);
        }
        [HttpGet("check")]
        public IActionResult CheckBookings([FromQuery] DateTime? reqStartTime, [FromQuery] DateTime? reqEndTime)
        {
            if (!reqStartTime.HasValue || !reqEndTime.HasValue)
                return BadRequest("StartTime and EndTime must be defined.");

            var overlappedBookings = _db.Bookings.Where(booking =>
                booking.StartTime < reqEndTime.Value && booking.EndTime > reqStartTime.Value
            ).ToList();

            var bookingResponseList = overlappedBookings.Select(_modelConverter.ConvertBookingResponse).ToList();
            var reponse = new BookingModel.OverlapResponse {
                ReqStartTime = reqStartTime,
                ReqEndTime = reqEndTime,
                OverlapBookings = bookingResponseList
            };

            return Ok(reponse);
            
        }

        //get by Id booking
        [HttpGet("{id}")]
        public IActionResult GetBookingById(int id)
        {
            var booking = _db.Bookings.FirstOrDefault(x => x.Id == id);
            return booking == null ? NotFound($"Booking with Id {id} doesn't exist") : Ok(_modelConverter.ConvertBookingResponse(booking));
        }

        //create booking
        [HttpPost]
        public IActionResult CreateBooking(BookingModel.CreateRequest createRequest)
        {
            
            if (createRequest == null) 
                return BadRequest("Must define a body");
            
            //join category to get hourlyRate
            var validConsole = _db.GameConsoles
                .Include(console => console.Category)
                .FirstOrDefault(g => g.Id == createRequest.ConsoleId);

            if (validConsole == null) 
                return BadRequest($"Console with Id {createRequest.ConsoleId} doesn't exist. ");


            if (!string.Equals(validConsole.CurrentStatus, "Available", StringComparison.OrdinalIgnoreCase))
                return Conflict($"Console with Id {createRequest.ConsoleId} is not available.");

            if (string.IsNullOrEmpty(createRequest.CustomerPhone)) 
                return BadRequest("Phone number is required");

            
            if (!createRequest.StartTime.HasValue) 
                return BadRequest("Start time must be defined");

            if (!createRequest.EndTime.HasValue) 
                return BadRequest("End time must be defined");


            //check if any conflict in time for both booking and session.
            //formula : A.StartTime < B.EndTime and A.EndTime > B.StartTime ( to check time overlapping between two interval)
          
            //check booking 
            int existingbookingCount = _db.Bookings
                .Where(booking => booking.ConsoleId == validConsole.Id &&
                booking.StartTime < createRequest.EndTime.Value && booking.EndTime > createRequest.StartTime.Value)
                .Count();
            
            //check sessions ( GameSession )
            int existingSessioncount = _db.GameSessions
                .Where(session => session.ConsoleId == validConsole.Id &&
                session.StartTime < createRequest.EndTime.Value && session.EndTime > createRequest.StartTime.Value)
                .Count();

            if (existingbookingCount > 0 || existingSessioncount > 0)
                return Conflict($"Time schedule conflict with {existingSessioncount} Sessions and {existingbookingCount} Bookings.Try to adjust the time.");
            

            if (createRequest.StartTime.Value > createRequest.EndTime.Value) 
                return BadRequest("Invalid time interval.");

            if (string.IsNullOrEmpty(createRequest.CustomerName))
                createRequest.CustomerName = "Unknown";
            
            //customer must pay atleast one session as deposit if they booking ( to make sure customer will arrive )
            string bookingStatus = "Pending";
            if (createRequest.DepositAmount > validConsole.Category.HourlyRate)
                bookingStatus = "Confirmed";

            TimeSpan duration = createRequest.EndTime.Value - createRequest.StartTime.Value;
            decimal totalHours = (decimal) duration.TotalHours;
            decimal totalAmount = totalHours * validConsole.Category.HourlyRate; 

            Booking newBooking = new Booking
            {
                ConsoleId = validConsole.Id,
                CustomerName = createRequest.CustomerName,
                CustomerPhone = createRequest.CustomerPhone,
                StartTime = createRequest.StartTime.Value,
                EndTime = createRequest.EndTime.Value,
                TotalHours = totalHours,
                TotalAmount = totalAmount,
                BookingStatus = bookingStatus,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                IsDeleted = false,

            };
            

            _db.Bookings.Add(newBooking);
            int status = _db.SaveChanges();
            return status > 0 ? Ok(_modelConverter.ConvertBookingResponse(newBooking)) : BadRequest("Couldn't create booking");
        }

        //[HttpPatch("{id}")]
        //public IActionResult UpdateBooking(Biij) { }

        //delete booking with soft delete support
        [HttpDelete("{id}")]
        public IActionResult DeleteBooking(int id, [FromQuery] bool isHardDelete = false)
        {
            var targetBooking = _db.Bookings.FirstOrDefault(b => b.Id == id);
            if (targetBooking == null)
                return NotFound($"Booking with Id {id} doesn't exist");

            //block hard delete if 
            if (isHardDelete)
            {
                int sessionCount = _db.GameSessions.Count(s => s.BookingId == id);

                if (sessionCount > 0)
                    return BadRequest($"Booking with Id {id} has {sessionCount} game session(s). Only soft delete is allowed.");

                _db.Bookings.Remove(targetBooking);
            }
            else
            {
                targetBooking.IsDeleted = true;
                targetBooking.UpdatedAt = DateTime.Now;
            }

            int status = _db.SaveChanges();
            return status > 0 ? Ok("Successfully deleted") : BadRequest("Failed to delete booking");
        }

    }
}
