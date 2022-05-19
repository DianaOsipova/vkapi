using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using vkapi.Models;

namespace vkapi
{
    public class ApplicationContext :DbContext
    {
        //введите свой пароль от pgAdmin
        private string postgreePassword = "YourPassword";
        public DbSet<Users> Users => Set<Users>();
        public DbSet<Groups> Groups  => Set<Groups>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql($"Host=localhost;Port=5432;Database=vkapidb;Username=postgres;Password={postgreePassword}");
        }
    }
}
