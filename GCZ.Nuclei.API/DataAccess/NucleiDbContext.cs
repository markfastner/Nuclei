using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class NucleiDbContext : DbContext
    {
        public NucleiDbContext(DbContextOptions<NucleiDbContext> options) : base(options)
        {

        }

        public DbSet<User> User { get; set; } = null!;
        public DbSet<Account> Account { get; set; } = null!;
        public DbSet<Log> Log { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
        }
    }
}
