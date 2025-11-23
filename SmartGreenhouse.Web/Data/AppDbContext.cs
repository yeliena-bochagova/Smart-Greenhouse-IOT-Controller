using Microsoft.EntityFrameworkCore;
using SmartGreenhouse.Web.Models;

namespace SmartGreenhouse.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<GreenhouseReading> Readings { get; set; }
        public DbSet<GreenhouseSettings> Settings { get; set; }
    }
}
