using System.Collections.Generic;
using BorderCrossing.Models;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace BorderCrossing
{
    public static class CountryStorage
    {
        public static List<Country> Countries { get; }

        static CountryStorage()
        {
            Countries = new List<Country>();
        }

        public static void Load(List<CountryJson> countries)
        {
            GeoJsonReader reader = new GeoJsonReader();

            foreach (var country in countries)
            {
                Countries.Add(new Country()
                {
                    Name = country.Name,
                    Region = country.Region,
                    Geom = reader.Read<Geometry>(country.Geom)
                });
            }
        }
    }
}
