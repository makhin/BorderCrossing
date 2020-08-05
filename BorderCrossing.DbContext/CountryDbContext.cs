using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BorderCrossing.DbContext
{
    public class CountryDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<Country> Countries { get; set; }

        public DbSet<Request> Requests { get; set; }

        public DbSet<CheckPoint> CheckPoints { get; set; }

        public DbSet<LocationHistoryFile> LocationHistoryFiles { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Request>()
                .HasOne(b => b.File)
                .WithOne(i => i.Request)
                .HasForeignKey<LocationHistoryFile>(b => b.RequestId);
        }

        public CountryDbContext(DbContextOptions<CountryDbContext> options) : base(options)
        {
        }
    }
}