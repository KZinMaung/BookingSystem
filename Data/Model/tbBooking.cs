using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Model
{
    [Table("tbBooking")]
    public class tbBooking
    {
        [Key]
        public int ID { get; set; }
        public string Code { get; set; }
        public int UserID { get; set; }
        public int ClassScheduleID { get; set; }
        public int UsedCredits { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? AccessTime { get; set; }
        public bool? IsDeleted { get; set; }

        public int PackageID { get; set; }
    }
}
