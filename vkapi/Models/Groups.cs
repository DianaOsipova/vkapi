using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vkapi.Models
{
    public class Groups
    {
        [Key]
        public string Id { get; set; }
        public List<Users> Users { get; set; } = new();
    }
}
