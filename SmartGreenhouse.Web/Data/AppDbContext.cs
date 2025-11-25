using Microsoft.EntityFrameworkCore;
using SmartGreenhouse.Web.Models;

namespace SmartGreenhouse.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options) 
        { 
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Sensor> Sensors { get; set; } = null!;
        public DbSet<Measurement> Measurements { get; set; } = null!;
        public DbSet<GreenhouseSettings> Settings { get; set; } = null!;
        public DbSet<Plant> Plants { get; set; } = null!;
    }
}
