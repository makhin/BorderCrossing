using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using Jil;
using NetTopologySuite.IO;

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
            //var optionsBuilder = new DbContextOptionsBuilder<CountryDbContext>();
            //optionsBuilder.UseSqlite(@"Data Source=C:\Users\omakhin\RiderProjects\BorderCrossing\BorderCrossing.Console\BorderCrossing.db", b => {
            //    b.UseNetTopologySuite();
            //});

            string connectionString = "Server=localhost;Database=BorderCrossing;Trusted_Connection=True;";

            var optionsBuilder2 = new DbContextOptionsBuilder<CountryDbContext>();
            optionsBuilder2.UseSqlServer(connectionString, b => {
                b.UseNetTopologySuite();
            });

            //var dbl = new CountryDbContext(optionsBuilder.Options);
            var dbs = new CountryDbContext(optionsBuilder2.Options);

            List<Country> countries = new List<Country>();

            GeoJsonWriter writer = new GeoJsonWriter();

            foreach (var country in dbs.Countries)
            {
                var c = new Country()
                {
                    Name = country.Name,
                    Region = country.Region,
                    Geom = writer.Write(country.Geom)
            };

                countries.Add(c);
                //dbl.Countries.Add(c);
                //dbl.SaveChanges();
            }

            var json = JSON.Serialize(countries);

            File.WriteAllText(@"c:\temp\c3.json", json);
        }
    }

    [DataContract]
    public class Country
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public short Region { get; set; }

        [DataMember]
        public string Geom { get; set; }
    }
}
