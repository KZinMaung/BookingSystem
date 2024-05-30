using API.Services.Scheduler;
using Azure;
using Core.Extension;
using Data.Model;
using Data.ViewModel;
using Data.ViewModel.Booking;
using Data.ViewModel.Package;
using Infra.Services;
using Infra.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace API.Services.Booking
{
    public class BookingBase : IBooking
    {
        private readonly AppDBContext _context;
        UnitOfWork _uow;
        private readonly IConnectionMultiplexer _redis;
        private readonly RefundJobScheduler _refundJobScheduler;

        public BookingBase(AppDBContext context, IConnectionMultiplexer redis, RefundJobScheduler refundJobScheduler)
        {
            this._context = context;
            this._uow = new UnitOfWork(_context);
            this._redis = redis;
            this._refundJobScheduler = refundJobScheduler;
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

        //Booking
        private async Task<ResponseModel> Book(int userID, int scheduleID)
        {
            ResponseModel response = new ResponseModel();

            if (IsAlreadyBooked(userID, scheduleID))
            {
                response.ReturnMessage = "User has already booked this class.";
                return response;
            }

            var schedule = await GetSchedule(scheduleID);
            var availablePk = await GetUserPurchasedPackage(userID, schedule.CountryID);

            if (availablePk.ID == 0)
            {
                response.ReturnMessage = "User does not have available packages to book the class";
                return response;
            }
            else if (availablePk.IsExpired)
            {
                response.ReturnMessage = "User's package is expired.";
                return response;
            }

            if (HasOverlapTime(userID, schedule))
            {
                response.ReturnMessage = "This schedule overlaps with user's booked schedules.";
                return response;
            }

            if (!HasEnoughCredits(schedule, availablePk))
            {
                response.ReturnMessage = "Remaining credits are not enough to book this schedule";
                return response;
            }

            if (!HasAvailableSlots(schedule))
            {
                response.ReturnMessage = "No available slots for this schedule. User has been added to waiting list.";
                await AddToWaitingList(userID, scheduleID, availablePk.ID);
                return response;
            }

            //can book
            bool isSuccess = await BookSchedule(userID, scheduleID, schedule, availablePk);
            if (isSuccess)
            {
                response.ReturnMessage = "Successfully booked!";
                return response;
            }
            else
            {
                response.ReturnMessage = "Something went wrong!";
                return response;
            }
        }

        private bool IsAlreadyBooked(int userID, int scheduleID)
        {
            return _uow.bookingRepo.GetAll().Any(a => a.IsDeleted != true && a.UserID == userID && a.ClassScheduleID == scheduleID);
        }

        private async Task<tbClassSchedule> GetSchedule(int scheduleID)
        {
            return await _uow.classScheduleRepo.GetAll().FirstOrDefaultAsync(a => a.IsDeleted != true && a.ID == scheduleID) ?? new tbClassSchedule();
        }

        private async Task<tbUserPurchasedPackage> GetUserPurchasedPackage(int userID, int countryID)
        {
            return await _uow.userPurchasedPackage.GetAll()
                .FirstOrDefaultAsync(a => a.IsDeleted != true && a.UserID == userID && a.CountryID == countryID) ?? new tbUserPurchasedPackage();
        }

        private bool HasOverlapTime(int userID, tbClassSchedule schedule)
        {
            var bookingSchedules = _uow.bookingRepo.GetAll()
                .Where(a => a.IsDeleted != true && a.UserID == userID)
                .Join(_uow.classScheduleRepo.GetAll().Where(a => a.IsDeleted != true),
                    b => b.ClassScheduleID,
                    s => s.ID,
                    (b, s) => s);

            return bookingSchedules.Any(bs => schedule.StartTime < bs.EndTime && schedule.EndTime > bs.StartTime);
        }

        private bool HasEnoughCredits(tbClassSchedule schedule, tbUserPurchasedPackage availablePk)
        {
            return schedule.CreditsRequired <= availablePk.RemainingCredits;
        }

        private bool HasAvailableSlots(tbClassSchedule schedule)
        {
            int bookingCount = _uow.bookingRepo.GetAll()
                .Count(a => a.IsDeleted != true && a.ClassScheduleID == schedule.ID && a.Status == "booked");

            return bookingCount < schedule.TotalSlots;
        }

        private async Task<bool> BookSchedule(int userID, int scheduleID, tbClassSchedule schedule, tbUserPurchasedPackage availablePk)
        {
            var entity = new tbBooking
            {
                UserID = userID,
                ClassScheduleID = scheduleID,
                UsedCredits = schedule.CreditsRequired,
                CreatedAt = MyExtension.getLocalTime(),
                AccessTime = MyExtension.getLocalTime(),
                UserPurchasedPackageID = availablePk.ID,
                Code = MyExtension.getUniqueNumber(),
                Status = "booked"
            };

            var result = await _uow.bookingRepo.InsertReturnAsync(entity);
            if (result == null)
                return false;

            availablePk.UsedCredits += schedule.CreditsRequired;
            availablePk.AccessTime = DateTime.Now;
            var a = await _uow.userPurchasedPackage.UpdateAsync(availablePk);
            if(a == null)
            {
                //rollback
                result.IsDeleted = true;
                result.AccessTime = DateTime.Now;
                await _uow.bookingRepo.UpdateAsync(result);
                return false;
            }

            return true;
        }

        private async Task AddToWaitingList(int userID, int scheduleID, int userPurchasedPackageID)
        {
            var waitingEntity = new tbWaitingList
            {
                UserID = userID,
                ClassScheduleID = scheduleID,
                CreatedAt = MyExtension.getLocalTime(),
                AccessTime = MyExtension.getLocalTime(),
                UserPurchasedPackageID = userPurchasedPackageID
            };

            await _uow.waitingListRepo.InsertReturnAsync(waitingEntity);

            tbClassSchedule cs = await GetScheduleByID(scheduleID);
            // Schedule the refund job for the class end time
            _refundJobScheduler.ScheduleRefundJob(scheduleID, cs.EndTime);
        }

        public async Task<ResponseModel> BookWithConcurrencyControl(int userID, int scheduleID)
        {
            ResponseModel response = new ResponseModel();
            var db = _redis.GetDatabase();
            string activeRequestsKey = "active:requests";
            string lockKey = "lock:active:requests";
            int maxConcurrentRequests = 5;

            // Acquire a lock
            bool lockAcquired = await db.LockTakeAsync(lockKey, Environment.MachineName, TimeSpan.FromSeconds(30));

            if (!lockAcquired)
            {
                response.ReturnMessage = "Too many requests. Please try again later.";
                return response;
            }

            try
            {
                // Get current active requests
                int currentActiveRequests = (int)await db.StringGetAsync(activeRequestsKey);

                if (currentActiveRequests >= maxConcurrentRequests)
                {
                    response.ReturnMessage = "Too many requests. Please try again later.";
                    return response;
                }

                // Increment the active request count
                await db.StringIncrementAsync(activeRequestsKey);

                // Call the Book method logic
                response = await Book(userID, scheduleID);

                // Simulate booking logic
                await Task.Delay(1000);

                // Decrement the active request count
                await db.StringDecrementAsync(activeRequestsKey);
            }
            finally
            {
                // Release the lock
                await db.LockReleaseAsync(lockKey, Environment.MachineName);
            }

            return response;
        }





    }


}



