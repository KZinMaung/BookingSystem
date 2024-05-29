using Data.Model;
using Data.ViewModel.Package;
using Infra.Services;

namespace API.Services.Package
{
    public interface IPackage
    {
        Task<Model<PackageVM>> GetPackages(int page, int pageSize);
        Task<Model<PurchasedPackageVM>> GetPurchasedPackages(int userID, int page, int pageSize);
    }
}
