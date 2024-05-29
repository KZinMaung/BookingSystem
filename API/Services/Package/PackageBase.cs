using Core.Extension;
using Data.Model;
using Data.ViewModel.Package;
using Infra.Services;
using Infra.UnitOfWork;
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

    }
}
