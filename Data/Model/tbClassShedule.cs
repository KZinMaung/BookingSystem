using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Model
{
    [Table("tbClassSchedule")]
    public class tbClassSchedule
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public int CountryID { get; set; }
        public int CreditsRequired { get; set; }
        public DateTime ScheduleDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationMinutes { get; set; }
        public int TotalSlots { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? AccessTime { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
