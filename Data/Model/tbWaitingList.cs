using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Model
{
    [Table("tbWaitingList")]
    public class tbWaitingList
    {
        [Key]
        public int ID { get; set; }
        public int UserID { get; set; }
        public int ClassScheduleID { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? AccessTime { get; set; }
        public bool? IsDeleted { get; set; }

        public int PackageID {  get; set; }
    }
}
