using Data.Model;
using Data.ViewModel;
using Data.ViewModel.Booking;
using Data.ViewModel.Package;
using Infra.Services;

namespace API.Services.Booking
{
    public interface IBooking
    {
        Task<Model<ScheduleVM>> GetAvailableSchedules(int page, int pageSize);

        Task<ResponseModel> BookWithConcurrencyControl(int userID, int scheduleID);

        Task<Model<BookingScheduleVM>> GetBookingSchedules(int userID, int page, int pageSize);
        Task<ResponseModel> CancelBooking(int bookingID);
        Task<ResponseModel> RefundWaitlistCredits(int scheduleID);
        Task<ResponseModel> CheckIn(int bookingId);

    }
}
