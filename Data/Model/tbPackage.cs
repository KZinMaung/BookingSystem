using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Model
{
    [Table("tbPackage")]
    public class tbPackage
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public int Credits { get; set; }
        public decimal Price { get; set; }
        public DateTime ExpiredDate { get; set; }
        public int CountryID { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? AccessTime { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
