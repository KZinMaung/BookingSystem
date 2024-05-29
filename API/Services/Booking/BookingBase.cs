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
            
            //CheckPackageToBook
            tbClassSchedule schedule =  _uow.classScheduleRepo.GetAll().Where(a => a.IsDeleted != true && a.ID == scheduleID).FirstOrDefault() ?? new tbClassSchedule();
            tbUserPurchasedPackage availablePk = _uow.userPurchasedPackage.GetAll().Where(a => a.IsDeleted != true && a.UserID == userID && a.CountryID == schedule.CountryID).FirstOrDefault() ?? new tbUserPurchasedPackage();
            if (availablePk.ID == 0)
            {
                response.ReturnMessage = "User does not have available packages to book the class";
                return response;
            }

            
            // Check for overlap
            
            List<tbClassSchedule> bookingSchedules = new List<tbClassSchedule>();
            var bookings = _uow.bookingRepo.GetAll()
                                            .Where(a => a.IsDeleted != true && a.UserID == userID).AsQueryable();
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


            //checkCredits
            if(schedule.CreditsRequired > availablePk.RemainingCredits)
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
                PackageID = availablePk.ID
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
                await _uow.bookingRepo.InsertReturnAsync(entity);

                //update used credits
                availablePk.UsedCredits += schedule.CreditsRequired;
                await _uow.userPurchasedPackage.UpdateAsync(availablePk);


                response.ReturnMessage = "Successfully booked!";
                return response;
            }
            else
            {
                entity.Status = "waiting";
                await _uow.bookingRepo.InsertReturnAsync(entity);

                //update used credits
                availablePk.UsedCredits += schedule.CreditsRequired;
                await _uow.userPurchasedPackage.UpdateAsync(availablePk);


                tbWaitingList waitingEntity = new tbWaitingList
                {
                    UserID = userID,
                    ClassScheduleID = scheduleID,
                    CreatedAt = MyExtension.getLocalTime(),
                    AccessTime = MyExtension.getLocalTime(),
                    PackageID = availablePk.ID
                };
                await _uow.waitingListRepo.InsertReturnAsync(waitingEntity);

                response.ReturnMessage = "User has been added to waiting list.";
                return response;

            }

        }


        }


    }



