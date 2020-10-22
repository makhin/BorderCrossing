using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel;
using BorderCrossing.Models;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Newtonsoft.Json;

namespace BorderCrossing
{
    public class CountryStorage
    {
        private static CountryStorage _instance;

        protected CountryStorage()
        {
            var appInstalledFolder = Package.Current.InstalledLocation;
            var assetsFolder = appInstalledFolder.GetFolderAsync("Assets").GetAwaiter().GetResult();
            var storageFile = assetsFolder.GetFileAsync("countries.json").GetAwaiter().GetResult();
            var json = Windows.Storage.FileIO.ReadTextAsync(storageFile).GetAwaiter().GetResult();
            List<CountryJson> countries = JsonConvert.DeserializeObject<List<CountryJson>>(json);

            var reader = new GeoJsonReader();

            foreach (var country in countries.Where(c => c.Region == 150)) //TODO Demo restriction
            {
                Countries.Add(new Country()
                {
                    Name = country.Name,
                    Region = country.Region,
                    Geom = reader.Read<Geometry>(country.Geom)
                });
            }
        }

        public static CountryStorage GetCountryStorage()
        {
            return _instance ??= new CountryStorage();
        }

        public List<Country> Countries { get; } = new List<Country>();
    }
}
