namespace BorderCrossing.Console
{
    using BorderCrossing.DbContext;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;

    public class CountryDbContextFactory : IDesignTimeDbContextFactory<CountryDbContext>
    {
        public CountryDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CountryDbContext>();
            optionsBuilder.UseSqlite("Data Source=BorderCrossing.db", b => {
                    b.MigrationsAssembly("BorderCrossing.Console");
                    b.UseNetTopologySuite();
                });

            return new CountryDbContext(optionsBuilder.Options);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CountryDbContext>();
            optionsBuilder.UseSqlite("Data Source=BorderCrossing.db", b => {
                b.UseNetTopologySuite();
            });

            string connectionString = "Server=localhost;Database=World;Trusted_Connection=True;";

            var optionsBuilder2 = new DbContextOptionsBuilder<CountryDbContext>();
            optionsBuilder2.UseSqlServer(connectionString, b => {
                b.UseNetTopologySuite();
            });

            var dbl = new CountryDbContext(optionsBuilder.Options);
            var dbs = new CountryDbContext(optionsBuilder2.Options);

        }
    }
}
