using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BorderCrossing.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Geometries;

namespace BorderCrossing.DbContext
{
    public interface IBorderCrossingRepository
    {
        List<Country> GetAllCountries();
        Task SaveLocationHistoryFileAsync(MemoryStream memoryStream, string fileName, Guid guid);
        void AddLocations(Dictionary<DateTime, Geometry> locations, string guid);
        Dictionary<DateTime, Geometry> GetLocations(string guid);
    }

    public class BorderCrossingRepository : IBorderCrossingRepository
    {
        private const string CountriesKey = "Countries";
        private readonly CountryDbContext _appDbContext;
        private readonly IMemoryCache _cache;

        public BorderCrossingRepository(CountryDbContext appDbContext, IMemoryCache cache)
        {
            _appDbContext = appDbContext;
            _cache = cache;
        }

        public List<Country> GetAllCountries()
        {
            if (!_cache.TryGetValue(CountriesKey, out List<Country> allCountries))
            {
                allCountries = _appDbContext.Countries.Where(c=>c.Region == 150).ToList();
                _cache.Set(CountriesKey, allCountries);
            }
            
            return allCountries;
        }

        public void AddLocations(Dictionary<DateTime, Geometry> locations, string guid)
        {
            _cache.Set(guid, locations, TimeSpan.FromMinutes(15));
        }
        
        public Dictionary<DateTime, Geometry> GetLocations(string guid)
        {
            return _cache.Get<Dictionary<DateTime, Geometry>>(guid);
        }

        public async Task SaveLocationHistoryFileAsync(MemoryStream memoryStream, string fileName, Guid guid)
        {
            _appDbContext.Database.SetCommandTimeout(180);
            var locationHistoryFile = new LocationHistoryFile
            {
                File = memoryStream.ToArray(),
                DateUpload = DateTime.Now,
                RequestId = guid,
                FileName = fileName
            };

            await _appDbContext.LocationHistoryFiles.AddAsync(locationHistoryFile);

            try
            {
                await _appDbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }

        }
    }
}