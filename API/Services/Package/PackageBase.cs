using Core.Extension;
using Data.Model;
using Data.ViewModel;
using Data.ViewModel.Package;
using Infra.Services;
using Infra.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Linq.Expressions;

namespace API.Services.Package
{
    public class PackageBase : IPackage
    {
        private readonly AppDBContext _context;
        UnitOfWork _uow;
        DateTime _now;
        public PackageBase(AppDBContext context)
        {
            this._context = context;
            this._uow = new UnitOfWork(_context);
            this._now = MyExtension.getLocalTime();
        }

        public async Task<Model<PackageVM>> GetPackages(int page, int pageSize)
        {
            var packages = _uow.packageRepo.GetAll()
                                            .Where(a => a.IsDeleted != true).AsQueryable();
            var countries = _uow.countryRepo.GetAll()
                                            .Where(a => a.IsDeleted != true).AsQueryable();

            var query = from package in packages
                        join country in countries on package.CountryID equals country.ID
                        select new PackageVM
                        {
                            ID = package.ID,
                            Name = package.Name,
                            Credits = package.Credits,
                            Price = package.Price,
                            ExpiredDate = package.ExpiredDate,
                            CountryName = country.Name
                        };

           
           
            var result = await PagingService<PackageVM>.getPaging(page, pageSize, query);
            return result;
        }

        public async Task<Model<PurchasedPackageVM>> GetPurchasedPackages(int userID, int page, int pageSize)
        {
            var purchasedPackages = _uow.userPurchasedPackage.GetAll()
                                            .Where(a => a.IsDeleted != true && a.UserID == userID).AsQueryable();
            var packages = _uow.packageRepo.GetAll()
                                            .Where(a => a.IsDeleted != true).AsQueryable();

            var query = from purchasedPackage in purchasedPackages
                        join package in packages on purchasedPackage.PackageID equals package.ID
                        select new PurchasedPackageVM
                        {
                            PackageID = purchasedPackage.PackageID,
                            PackageName = package.Name,
                            Credits = package.Credits,
                            Price = package.Price,
                            CountryID = package.CountryID,
                            PurchasedDate = purchasedPackage.PurchasedDate,
                            ExpiredDate = purchasedPackage.ExpiredDate,
                            TotalCredits = purchasedPackage.TotalCredits,
                            UsedCredits = purchasedPackage.UsedCredits ?? 0
                        };


            var result = await PagingService<PurchasedPackageVM>.getPaging(page, pageSize, query);
            return result;
        }


        public async Task<ResponseModel> PurchasePackage(int userID, int packageID)
        {
            ResponseModel response = new ResponseModel();
            tbPackage package = await _uow.packageRepo.GetAll().Where(a => a.IsDeleted != true && a.ID == packageID).FirstOrDefaultAsync() ?? new tbPackage();
            if(package.ID == 0)
            {
                response.ReturnMessage = "The package does not exist";
                return response;
            }
            tbUserPurchasedPackage pp = new tbUserPurchasedPackage
            {
                UserID = userID,
                PackageID = packageID,
                PurchasedDate = DateTime.Now,
                ExpiredDate = package.ExpiredDate,
                CreatedAt = DateTime.Now,
                IsDeleted = false,
                AccessTime = DateTime.Now,
                TotalCredits = package.Credits,
                UsedCredits = 0,
                CountryID = package.CountryID
            };
            var result = await _uow.userPurchasedPackage.InsertReturnAsync(pp);
            if(result != null)
            {
                var isPaid = PaymentCharge();
                if(isPaid)
                {
                    response.ReturnMessage = "User has purchased the package successfully.";
                    return response;
                }
            }
        
            response.ReturnMessage = "Failed purchase!";
            return response;
            
        }

        private bool PaymentCharge()
        {
            return true;
        }


    }
}
