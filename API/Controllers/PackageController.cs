using API.Services.Package;
using API.Services.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    public class PackageController : ControllerBase
    {

        IPackage _ipackage;

        public PackageController(IPackage ipackage)
        {
            this._ipackage = ipackage;
        }

        [HttpGet("api/package/get_packages")]
        public async Task<IActionResult> GetPackages(int page = 1, int pageSize = 10)
        {
            var result = await this._ipackage.GetPackages(page, pageSize);
            return Ok(result);
        }


        [HttpGet("api/package/get_purchased_packages")]
        public async Task<IActionResult> GetPurchasedPackages(int userID , int page = 1, int pageSize = 10)
        {
            var result = await this._ipackage.GetPurchasedPackages(userID, page, pageSize);
            return Ok(result);
        }

        [HttpGet("api/package/purchase_package")]
        public async Task<IActionResult> PurchasePackage(int userID, int packageID)
        {
            var result = await this._ipackage.PurchasePackage(userID, packageID);
            return Ok(result);
        }

    }
}
