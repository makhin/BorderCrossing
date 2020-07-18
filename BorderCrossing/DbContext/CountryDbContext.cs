using Microsoft.EntityFrameworkCore;

namespace BorderCrossing.DbContext
{
    public class CountryDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<Country> Countries { get; set; }
        
        public DbSet<LocationHistoryFile> UsageHistory { get; set; }

        public CountryDbContext(DbContextOptions<CountryDbContext> options) : base(options)
        {
        }
    }
}