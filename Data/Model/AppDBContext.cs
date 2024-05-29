using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Data.Model
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public virtual DbSet<tbBooking> tbBooking { get; set; }
        public virtual DbSet<tbClassSchedule> tbClassSchedule { get; set; }
        public virtual DbSet<tbCountry> tbCountry { get; set; }
        public virtual DbSet<tbPackage> tbPackage { get; set; }
        public virtual DbSet<tbUser> tbUser { get; set; }
        public virtual DbSet<tbUserPurchasedPackage> tbUserPurchasedPackage { get; set; }
        public virtual DbSet<tbWaitingList> tbWaitingList { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
        }
    }
}
