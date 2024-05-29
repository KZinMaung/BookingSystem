using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Model
{
    [Table("tbUser")]
    public class tbUser
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool? IsEmailVerified { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? AccessTime { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
