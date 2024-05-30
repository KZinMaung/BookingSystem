using Azure;
using Core.Extension;
using Data.Model;
using Data.ViewModel;
using Data.ViewModel.Booking;
using Data.ViewModel.Package;
using Infra.Services;
using Infra.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Booking
{
    public class BookingBase : IBooking
    {
        private readonly AppDBContext _context;
        UnitOfWork _uow;
       
        public BookingBase(AppDBContext context)
        {
            this._context = context;
            this._uow = new UnitOfWork(_context);
           
        }

        public async Task<Model<ScheduleVM>> GetAvailableSchedules(int page, int pageSize)
        {
            var countries = _uow.countryRepo.GetAll()
                                            .Where(a => a.IsDeleted != true).AsQueryable();
            var schedules = _uow.classScheduleRepo.GetAll()
                                            .Where(a => a.IsDeleted != true).AsQueryable();

            var query = from country in countries
                        join schedule in schedules on country.ID equals schedule.CountryID into countrySchedules
                        select new ScheduleVM
                        {
                            Country = country,
                            ClassSchedules = countrySchedules.ToList()
                        };



            var result = await PagingService<ScheduleVM>.getPaging(page, pageSize, query);
            return result;
        }


        public async Task<ResponseModel> Book(int userID, int scheduleID)
        {
            ResponseModel response= new ResponseModel();
            //check already booked
            var alreadyBooked = _uow.bookingRepo.GetAll().Where(a => a.IsDeleted != true && a.UserID == userID && a.ClassScheduleID == scheduleID).Any();
            if(alreadyBooked)
            {
                response.ReturnMessage = "User has alredy booked this class.";
                return response;
            }


            //CheckPackageToBook
            tbClassSchedule schedule =  await _uow.classScheduleRepo.GetAll().Where(a => a.IsDeleted != true && a.ID == scheduleID).FirstOrDefaultAsync() ?? new tbClassSchedule();
            tbUserPurchasedPackage availablePk = await _uow.userPurchasedPackage.GetAll().Where(a => a.IsDeleted != true && a.UserID == userID && a.CountryID == schedule.CountryID).FirstOrDefaultAsync() ?? new tbUserPurchasedPackage();
            if (availablePk.ID == 0)
            {
                response.ReturnMessage = "User does not have available packages to book the class";
                return response;
            }
            else if( availablePk.IsExpired)
            {
                response.ReturnMessage = "User's package is expired.";
                return response;
            }

            
            // Check for overlap time
            
            List<tbClassSchedule> bookingSchedules = new List<tbClassSchedule>();
            var bookings = _uow.bookingRepo.GetAll()
                                            .Where(a => a.IsDeleted != true && a.UserID == userID).AsQueryable();
            if(bookings.Any())
            {
                var schedules = _uow.classScheduleRepo.GetAll()
                                           .Where(a => a.IsDeleted != true).AsQueryable();

                var query = from b in bookings
                            join s in schedules on b.ClassScheduleID equals s.ID
                            select s;

                bookingSchedules = await query.ToListAsync();


                foreach (var bs in bookingSchedules)
                {
                    if (schedule.StartTime < bs.EndTime && schedule.EndTime > bs.StartTime)
                    {
                        response.ReturnMessage = "This schedule is overlap with user's booking schedules";
                        return response;
                    }
                }

            }


            //checkCredits
            if (schedule.CreditsRequired > availablePk.RemainingCredits)
            {
                response.ReturnMessage = "Remaining credits are not enough to book this schedule";
                return response;
            }

            //can booked
            tbBooking entity = new tbBooking
            {
                UserID = userID,
                ClassScheduleID = scheduleID,
                UsedCredits = schedule.CreditsRequired,
                CreatedAt = MyExtension.getLocalTime(),
                AccessTime = MyExtension.getLocalTime(),
                UserPurchasedPackageID = availablePk.ID,
                Code  = MyExtension.getUniqueNumber(),
            };


            //checkAvailableSlots
            bool reachLimit = false;
            int bookingCount = _uow.bookingRepo.GetAll().Where(a => a.IsDeleted != true && a.ClassScheduleID == scheduleID && a.Status == "booked").Count();
            if(bookingCount >= schedule.TotalSlots)
            {
                reachLimit = true;
            }

            
            if (!reachLimit)
            {
                entity.Status = "booked";
                _ = await _uow.bookingRepo.InsertReturnAsync(entity);

                //update used credits
                availablePk.UsedCredits += schedule.CreditsRequired;
                availablePk.AccessTime = DateTime.Now;
                _ = await _uow.userPurchasedPackage.UpdateAsync(availablePk);


                response.ReturnMessage = "Successfully booked!";
                return response;
            }
            else
            {
                entity.Status = "waiting";
                _ = await _uow.bookingRepo.InsertReturnAsync(entity);

                //update used credits
                availablePk.UsedCredits += schedule.CreditsRequired;
                availablePk.AccessTime = DateTime.Now;
                _ = await _uow.userPurchasedPackage.UpdateAsync(availablePk);


                tbWaitingList waitingEntity = new tbWaitingList
                {
                    UserID = userID,
                    ClassScheduleID = scheduleID,
                    CreatedAt = MyExtension.getLocalTime(),
                    AccessTime = MyExtension.getLocalTime(),
                    UserPurchasedPackageID = availablePk.ID
                };
                _ = await _uow.waitingListRepo.InsertReturnAsync(waitingEntity);

                response.ReturnMessage = "User has been added to waiting list.";
                return response;

            }

        }


        public async Task<Model<BookingScheduleVM>> GetBookingSchedules(int userID, int page, int pageSize)
        {
            var bookings = _uow.bookingRepo.GetAll()
                                            .Where(a => a.IsDeleted != true && a.UserID == userID && a.Status == "booked").AsQueryable();
            var schedules = _uow.classScheduleRepo.GetAll()
                                            .Where(a => a.IsDeleted != true).AsQueryable();

            var query = from b in bookings
                        join s in schedules on b.ClassScheduleID equals s.ID 
                        select new BookingScheduleVM
                        {
                            Booking = b,
                            Schedule = s
                        };


            var result = await PagingService<BookingScheduleVM>.getPaging(page, pageSize, query);
            return result;
        }

        private async Task<tbBooking> GetByID(int bookingID)
        {
            tbBooking b = await _uow.bookingRepo.GetAll().Where(a => a.IsDeleted != true && a.ID == bookingID).FirstOrDefaultAsync() ?? new tbBooking();
            return b;
        }

        private async Task<tbBooking> DeleteBooking(tbBooking b)
        {
            b.IsDeleted = true;
            b.AccessTime = DateTime.Now;
            var upatedEntity = await _uow.bookingRepo.UpdateAsync(b);
            return upatedEntity;
        }

        private async Task<List<tbWaitingList>> GetWaitingList(int scheduleID)
        {
            List<tbWaitingList> waitingList = await _uow.waitingListRepo.GetAll().Where(a => a.IsDeleted != true && a.ClassScheduleID == scheduleID).OrderBy(a => a.CreatedAt).ToListAsync();
            return waitingList;
        }

        private async Task<tbWaitingList> RemoveFromWaitingList(tbWaitingList firstEntity)
        {
            firstEntity.IsDeleted = true;
            firstEntity.AccessTime = DateTime.Now;
            var updatedFirstEntity = await _uow.waitingListRepo.UpdateAsync(firstEntity);
            return updatedFirstEntity;
        }

        private async Task<tbBooking> GetByIDCombination(int userID, int scheduleID)
        {
            tbBooking b = await _uow.bookingRepo.GetAll().Where(a => a.UserID == userID && a.ClassScheduleID == scheduleID && a.IsDeleted != true).FirstOrDefaultAsync() ?? new tbBooking();
            return b;
        }

        private async Task<tbBooking> MarkAsBooked(tbBooking waitingBooking)
        {
            waitingBooking.Status = "booked";
            waitingBooking.AccessTime = DateTime.Now;
            var updatedEntity = await _uow.bookingRepo.UpdateAsync(waitingBooking);
            return updatedEntity;
        }

        private async Task<tbClassSchedule> GetScheduleByID(int id)
        {
            tbClassSchedule s = await _uow.classScheduleRepo.GetAll().Where(a => a.ID == id && a.IsDeleted != true).FirstOrDefaultAsync() ?? new tbClassSchedule();
            return s;
        }

        private bool IsAllowedToRefund(DateTime startTime, DateTime cancellationTime)
        {
            return startTime.Subtract(cancellationTime).TotalHours >= 4;

        }

        private async Task<tbUserPurchasedPackage> GetPurchasedPackageByID(int id)
        {
            tbUserPurchasedPackage usedPackage = await _uow.userPurchasedPackage.GetAll().Where(a => a.IsDeleted != true && a.ID == id).FirstOrDefaultAsync() ?? new tbUserPurchasedPackage();
            return usedPackage;
        }

        private async Task<tbUserPurchasedPackage> Refund(tbUserPurchasedPackage usedPackage, int credits)
        {
            
            usedPackage.UsedCredits -= credits;
            usedPackage.AccessTime = DateTime.Now;
            var updatedEntity = await _uow.userPurchasedPackage.UpdateAsync(usedPackage);
            return updatedEntity;
        }
        public async Task<ResponseModel> CancelBooking(int bookingID)
        {
            ResponseModel response = new ResponseModel();
            //check booking exist
            tbBooking b = await GetByID(bookingID);
            if (b.ID == 0)
            {
                response.ReturnMessage = "The booking does not exist";
                return response;
            }

            b = await DeleteBooking(b);

            //AddToBookedList
            List<tbWaitingList> waitingList = await GetWaitingList(b.ClassScheduleID);
            if (waitingList.Any())
            {
                var firstEntity = waitingList.FirstOrDefault() ?? new tbWaitingList();
                _ = await RemoveFromWaitingList(firstEntity);

                tbBooking waitingBooking = await GetByIDCombination(firstEntity.UserID, firstEntity.ClassScheduleID);
                if (waitingBooking.ID != 0)
                {
                    _ = await MarkAsBooked(waitingBooking);
                }
            }


            //checkToRefund
            tbClassSchedule bookingSchedule = await GetScheduleByID(b.ClassScheduleID);
            DateTime cancellationTime = DateTime.Now;
            bool isRefund = IsAllowedToRefund(bookingSchedule.StartTime, cancellationTime);
            if (isRefund)
            {
                tbUserPurchasedPackage usedPackage = await GetPurchasedPackageByID(b.UserPurchasedPackageID);
                if (usedPackage.ID != 0)
                {
                    await Refund(usedPackage, b.UsedCredits);
                    response.ReturnMessage = "The booking has been cancelled and refund to the user successfully!";
                    return response;
                }
            }
            
            response.ReturnMessage = "The booking has been cancelled successfully!";
            return response; 
            
            
        }

        public async Task<ResponseModel> RefundWaitlistCredits(int scheduleID)
        {
            ResponseModel response = new ResponseModel();
            tbClassSchedule s = await GetScheduleByID(scheduleID);
            if (DateTime.Now <= s.EndTime)
            {
                response.ReturnMessage = "This schedule does not end yet!";
                return response;
            }

            List<tbWaitingList> waitingList = await GetWaitingList(scheduleID);
            foreach(var w in waitingList)
            {
                var userPackage = await GetPurchasedPackageByID(w.UserPurchasedPackageID);
                _ = await Refund(userPackage, s.CreditsRequired);
            }

            response.ReturnMessage = "Successfully refund to the waiting list's packages!";
            return response;
        }

        private async Task<tbBooking> UpdateBookingStatus(tbBooking b)
        {
            b.Status = "checkin";
            b.AccessTime = DateTime.Now;
            var updatedEntity = await _uow.bookingRepo.UpdateAsync(b);
            return updatedEntity;
        }
        public async Task<ResponseModel> CheckIn(int bookingID)
        {
            ResponseModel response = new ResponseModel();
            //check class is starting
            tbBooking b = await GetByID(bookingID);
            if(b.ID == 0)
            {
                response.ReturnMessage = "The booking does not exist.";
                return response;
            }
            tbClassSchedule s = await GetScheduleByID(b.ClassScheduleID);
            if(s.ID == 0)
            {
                response.ReturnMessage = "The booked class does not exist.";
                return response;
            }

            DateTime currentTime = DateTime.Now;
            if (!(currentTime >= s.StartTime && currentTime <= s.EndTime))
            {
                response.ReturnMessage = "The booked class does not start yet or may be end.";
                return response;
            }

            _ = await UpdateBookingStatus(b);
            response.ReturnMessage = "Successfully check in to the booked class";
            return response;
        }

    }


}



