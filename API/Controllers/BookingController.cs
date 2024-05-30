using API.Services.Booking;
using API.Services.Package;
using Data.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{

    [Authorize]
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

        [HttpPost("api/booking/book_with_concurrency_control")]
        public async Task<IActionResult> BookWithConcurrencyControl(int userID, int scheduleID)
        {
            var result = await this._ibooking.BookWithConcurrencyControl(userID, scheduleID);
            return Ok(result);
        }

        [HttpGet("api/booking/get_booking_schedules")]
        public async Task<IActionResult> GetBookingSchedules(int userID, int page = 1, int pageSize = 10)
        {
            var result = await this._ibooking.GetBookingSchedules(userID, page, pageSize);
            return Ok(result);
        }

        [HttpPost("api/booking/cancel_booking")]
        public async Task<IActionResult> CancelBooking(int bookingID)
        {
            var result = await this._ibooking.CancelBooking(bookingID);
            return Ok(result);
        }

        [HttpPost("api/booking/refund_waitlist_credits")]
        public async Task<IActionResult> RefundWaitlistCredits(int scheduleID)
        {
            var result = await this._ibooking.RefundWaitlistCredits(scheduleID);
            return Ok(result);
        }

        [HttpPost("api/booking/check_in")]
        public async Task<IActionResult> CheckIn(int bookingID)
        {
            var result = await this._ibooking.CheckIn(bookingID);
            return Ok(result);
        }

    }
}
