using Data.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.ViewModel.Booking
{
    public class BookingScheduleVM
    {
        public tbBooking Booking { get; set; }
        public tbClassSchedule Schedule { get; set; }
    }
}
