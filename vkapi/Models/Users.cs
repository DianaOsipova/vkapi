using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vkapi.Models
{
    public  class Users
    {
        [Key]
        public long Id { get; set; }
        public string Birthday { get; set; }
        public List<Groups> Groups { get; set; } = new();
    }
}
