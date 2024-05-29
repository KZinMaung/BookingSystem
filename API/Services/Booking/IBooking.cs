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

        Task<ResponseModel> Book(int userID, int scheduleID);
    }
}
