using API.Services.Booking;
using API.Services.Package;
using Data.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    
    [ApiController]
    public class BookingController : ControllerBase
    {
        IBooking _ibooking;

        public BookingController(IBooking ibooking)
        {
            this._ibooking = ibooking;
        }

        [HttpGet("api/booking/get_availabe_shedules")]
        public async Task<IActionResult> GetAvailableSchedules(int page = 1, int pageSize = 10)
        {
            var result = await this._ibooking.GetAvailableSchedules(page, pageSize);
            return Ok(result);
        }

        [HttpPost("api/booking/book")]
        public async Task<IActionResult> Book(int userID, int scheduleID)
        {
            var result = await this._ibooking.Book(userID, scheduleID);
            return Ok(result);
        }
    }
}
