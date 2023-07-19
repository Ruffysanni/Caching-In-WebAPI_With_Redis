using Implementing_Caching_In_WebAPI_With_Redis.Models;
using Microsoft.EntityFrameworkCore;

namespace Implementing_Caching_In_WebAPI_With_Redis.Data_Access_Layer
{
    public class DriverDbContext : DbContext
    {
        public DriverDbContext(DbContextOptions<DriverDbContext>options) : base(options)
        {
        }
        public DbSet<Driver> Drivers { get; set; }
    }
}
