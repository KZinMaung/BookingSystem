using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.ViewModel.Package
{
    public class PackageVM
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Credits { get; set; }
        public decimal Price { get; set; }
        public DateTime ExpiredDate { get; set; }
        public string CountryName {  get; set; }
        
    }
}
