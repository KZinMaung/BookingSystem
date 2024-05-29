using Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.ViewModel.Package
{
    public class PurchasedPackageVM
{
    public int PackageID { get; set; }
    public string PackageName { get; set; }
    public int Credits { get; set; }
    public decimal Price { get; set; }
    public int CountryID { get; set; }
    public DateTime PurchasedDate { get; set; }
    public DateTime ExpiredDate { get; set; }
    public int TotalCredits { get; set; }
    public int UsedCredits { get; set; }
    public int RemainingCredits => TotalCredits - UsedCredits;
    public bool IsExpired => DateTime.Now > ExpiredDate;
}
}
