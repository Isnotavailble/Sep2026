using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sep2026.GameShopWebApi.Models;
using Sep2026.GameShopWebApi.Utils;
using Microsoft.EntityFrameworkCore;
using Sep2026.QuizConsoleApp.Database.GameShopAppDbContext;
using Microsoft.IdentityModel.Tokens;

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

            var overlappedBookings = _db.Bookings
                .Where(booking =>
                       booking.StartTime < reqEndTime.Value &&
                       booking.EndTime > reqStartTime.Value &&
                       booking.IsDeleted == false
                       )
                .ToList();

            var overlapBookingResponses = overlappedBookings.Select(_modelConverter.ConvertBookingResponse).ToList();
            var reponse = new BookingModel.OverlapResponse {
                ReqStartTime = reqStartTime,
                ReqEndTime = reqEndTime,
                OverlapBookings = overlapBookingResponses
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
                .Count(booking => 
                    booking.ConsoleId == validConsole.Id &&
                    booking.StartTime < createRequest.EndTime.Value &&
                    booking.EndTime > createRequest.StartTime.Value);

            //check sessions ( GameSession )
            int existingSessioncount = _db.GameSessions
                .Count(session => 
                    session.ConsoleId == validConsole.Id &&
                    session.StartTime < createRequest.EndTime.Value &&
                    session.EndTime > createRequest.StartTime.Value);

            if (existingbookingCount > 0 || existingSessioncount > 0)
                return Conflict($"Time schedule conflict with {existingSessioncount} Sessions and {existingbookingCount} Bookings.Try to adjust the time.");


            if (createRequest.StartTime.Value > createRequest.EndTime.Value)
                return BadRequest("Invalid time interval.");

            if (string.IsNullOrEmpty(createRequest.CustomerName))
                createRequest.CustomerName = "Unknown";

            //customer must pay atleast one session as deposit if they booking ( to make sure customer will arrive )
            //note : if customer paid valid desposit ammount,keep that seat for this booking.
            string bookingStatus = "Pending";
            if (createRequest.DepositAmount > validConsole.Category.HourlyRate)
            {
                bookingStatus = "Confirmed";
                validConsole.CurrentStatus = "Taken";
            }
                

            TimeSpan duration = createRequest.EndTime.Value - createRequest.StartTime.Value;
            decimal totalHours = (decimal)duration.TotalHours;
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

        //update booking 
        [HttpPatch("{id}")]
        public IActionResult UpdateBooking(int id, BookingModel.UpdateRequest updateRequest)
        {
            bool hasChanges = false; //to track the validated changes.
            var targetBooking = _db.Bookings.FirstOrDefault(x => x.Id == id);

            if (targetBooking == null)
                return BadRequest($"Booking with Id {id} is not found.");

            //block dead end state (cancelled and completed )
            if (targetBooking.BookingStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) ||
                targetBooking.BookingStatus.Equals("Completed",StringComparison.OrdinalIgnoreCase))

                return BadRequest($"Booking is already {targetBooking.BookingStatus.ToLower()}.");


            //update customerName
            if (!string.IsNullOrWhiteSpace(updateRequest.CustomerName))
            {
                targetBooking.CustomerName = updateRequest.CustomerName;
                hasChanges = true;
            }

            //update customer phone
            if (!string.IsNullOrWhiteSpace(updateRequest.CustomerPhone))
            {
                targetBooking.CustomerPhone = updateRequest.CustomerPhone.Trim();
                hasChanges = true;
            }

            //update booking status
            //note : pending -> booking only 
            if (!string.IsNullOrWhiteSpace(updateRequest.BookingStatus))
            {
                //check valid enum first
                updateRequest.BookingStatus = updateRequest.BookingStatus.Trim();
                if (!IsValidBookingStatus(updateRequest.BookingStatus))
                    return BadRequest("Invalid booking status.");

                targetBooking.BookingStatus = updateRequest.BookingStatus;
                hasChanges = true;

            }

            //update deposite
            //note : BookingStatus in this line will only left Pending and Confirmed ( Cancelled is already blocked )
            // Minmum deposite value is price per hour ( hourlyRate )
            if (updateRequest.DepositAmount.HasValue)
            {
                decimal hourlyRate = _db.GameConsoles
                    .Include(gc => gc.Category)
                    .FirstOrDefault(x => x.Id == targetBooking.ConsoleId)?.Category?.HourlyRate ?? 0;

                if (updateRequest.DepositAmount.Value < hourlyRate)
                    return BadRequest($"Minimum deposit is {hourlyRate}.");

                if (updateRequest.DepositAmount.Value > targetBooking.TotalAmount)
                    return BadRequest("Deposit cannot exceed total amount.");

                targetBooking.DepositAmount = updateRequest.DepositAmount.Value;
                hasChanges = true;

            }
            //update start and end time or schedule 
            //note : customers always change their mind XD
            if (updateRequest.StartTime.HasValue && updateRequest.EndTime.HasValue)
            {
                // find overlap booking for this console.
                int bookingOverlapCount = _db.Bookings
                    .Count(booking =>
                        booking.Id != targetBooking.Id && 
                        booking.StartTime < updateRequest.EndTime &&
                        booking.EndTime > updateRequest.StartTime &&
                        booking.ConsoleId == targetBooking.ConsoleId &&
                        booking.BookingStatus != "Completed" &&
                        booking.BookingStatus != "Cancelled" &&
                        booking.IsDeleted == false
                    );

                if (bookingOverlapCount > 0)
                    return Conflict($"Cannot update booking time.{bookingOverlapCount} bookings already booked.");

                targetBooking.StartTime = updateRequest.StartTime.Value;
                targetBooking.EndTime = updateRequest.EndTime.Value;
                
                //Recalculate time and totalAmmount
                TimeSpan duration = updateRequest.EndTime.Value - updateRequest.StartTime.Value;
                decimal totalHours = (decimal) duration.TotalHours;

                decimal? hourlyRate = _db.GameConsoles
                    .Include(c => c.Category)
                    .FirstOrDefault(c => c.Id == targetBooking.ConsoleId)?.Category?.HourlyRate ?? 0;

                if (!hourlyRate.HasValue)
                    return NotFound("Invalid console category");

                decimal totalAmount = totalHours * hourlyRate.Value;

                //update totalhours and total amount 
                targetBooking.TotalAmount = totalAmount;
                targetBooking.TotalHours = totalHours;
                hasChanges = true;
            }

            if (hasChanges) {
                targetBooking.UpdatedAt = DateTime.Now;
            }

            int status = _db.SaveChanges();
            return status > 0 ?  Ok(_modelConverter.ConvertBookingResponse(targetBooking)) : BadRequest("Unable to update your request.");

        }

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

        //helper functions
        // to check the valid type with enum
        private bool IsValidBookingStatus(string inputType)
        {
            List<BookingModel.StatusTypes> types = Enum.GetValues<BookingModel.StatusTypes>().ToList();
            int count = types.Count(t => t.ToString().Equals(inputType, StringComparison.OrdinalIgnoreCase));

            return count > 0;
        }

    }
}
