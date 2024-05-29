using Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.ViewModel.Booking
{
    public class ScheduleVM
    {
        public tbCountry Country { get; set; }
        public List<tbClassSchedule> ClassSchedules { get; set; }  = new List<tbClassSchedule>();
    }
}
