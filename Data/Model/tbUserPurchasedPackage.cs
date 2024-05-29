using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Model
{
    [Table("tbUserPurchasedPackage")]
    public class tbUserPurchasedPackage
    {
        [Key]
        public int ID { get; set; }
        public int UserID { get; set; }
        public int PackageID { get; set; }
        public DateTime PurchasedDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? AccessTime { get; set; }
        public int TotalCredits { get; set; }
        public int? UsedCredits { get; set; }

        public int CountryID { get;set; }

        [NotMapped]
        public int RemainingCredits => TotalCredits - UsedCredits ?? 0;

        [NotMapped]
        public bool IsExpired => DateTime.Now > ExpiredDate;
    }
}
