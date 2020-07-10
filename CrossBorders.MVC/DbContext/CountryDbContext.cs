using Microsoft.EntityFrameworkCore;

namespace CrossBorders.MVC.DbContext
{
    public class CountryDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<Country> Countries { get; set; }
        
        public DbSet<UsageHistory> UsageHistory { get; set; }

        public CountryDbContext(DbContextOptions<CountryDbContext> options) : base(options)
        {
        }
    }
}