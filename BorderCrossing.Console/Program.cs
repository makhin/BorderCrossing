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
            optionsBuilder.UseSqlite("Data Source=country.db", b => {
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
        }
    }
}
