using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Model
{
    [Table("tbCountry")]
    public class tbCountry
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? AccessTime { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
