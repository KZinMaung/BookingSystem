using Core.Interface;
using Data.Model;
using Infra.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Infra.UnitOfWork
{
    public class UnitOfWork
    {
        private AppDBContext _ctx;
        private IRepository<tbBooking> _bookingRepo;
        private IRepository<tbClassSchedule> _classScheduleRepo;
        private IRepository<tbCountry> _countryRepo;
        private IRepository<tbPackage> _packageRepo;
        private IRepository<tbUser> _userRepo;
        private IRepository<tbUserPurchasedPackage> _userPurchasedPackage;
        private IRepository<tbWaitingList> _waitingListRepo;

        public UnitOfWork(AppDBContext ctx)
        {
            _ctx = ctx;
        }
        public UnitOfWork()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
              .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
              .AddJsonFile("appsettings.json")
              .Build();
            var contextOptions = new DbContextOptionsBuilder<AppDBContext>()
              .UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
              .Options;
            _ctx = new AppDBContext(contextOptions);

        }

        ~UnitOfWork()
        {
            _ctx.Dispose();
        }

        public IRepository<tbBooking> bookingRepo
        {
            get
            {
                if (_bookingRepo == null)
                {
                    _bookingRepo = new Repository<tbBooking>(_ctx);
                }
                return _bookingRepo;
            }
        }


        public IRepository<tbClassSchedule> classScheduleRepo
        {
            get
            {
                if (_classScheduleRepo == null)
                {
                    _classScheduleRepo = new Repository<tbClassSchedule>(_ctx);
                }
                return _classScheduleRepo;
            }
        }


        public IRepository<tbCountry> countryRepo
        {
            get
            {
                if (_countryRepo == null)
                {
                    _countryRepo = new Repository<tbCountry>(_ctx);
                }
                return _countryRepo;
            }
        }

        public IRepository<tbPackage> packageRepo
        {
            get
            {
                if (_packageRepo == null)
                {
                    _packageRepo = new Repository<tbPackage>(_ctx);
                }
                return _packageRepo;
            }
        }

        public IRepository<tbUser> userRepo
        {
            get
            {
                if (_userRepo == null)
                {
                    _userRepo = new Repository<tbUser>(_ctx);
                }
                return _userRepo;
            }
        }


        public IRepository<tbUserPurchasedPackage> userPurchasedPackage
        {
            get
            {
                if (_userPurchasedPackage == null)
                {
                    _userPurchasedPackage = new Repository<tbUserPurchasedPackage>(_ctx);
                }
                return _userPurchasedPackage;
            }
        }

        public IRepository<tbWaitingList> waitingListRepo
        {
            get
            {
                if (_waitingListRepo == null)
                {
                    _waitingListRepo = new Repository<tbWaitingList>(_ctx);
                }
                return _waitingListRepo;
            }
        }

    }
}
