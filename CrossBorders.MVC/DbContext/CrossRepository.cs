using System;
using System.Collections.Generic;
using System.Linq;
using CrossBorders.MVC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Geometries;

namespace CrossBorders.MVC.DbContext
{
    public interface ICrossRepository
    {
        IEnumerable<Country> GetAllCountries();
        void SaveUsageHistory(UsageHistory usageHistory);
        string AddLocations(Dictionary<DateTime, Geometry> locations);
        Dictionary<DateTime, Geometry> GetLocations(string guid);
    }

    public class CrossRepository : ICrossRepository
    {
        private const string CountriesKey = "Countries";
        private readonly CountryDbContext _appDbContext;
        private readonly IMemoryCache _cache;

        public CrossRepository(CountryDbContext appDbContext, IMemoryCache cache)
        {
            _appDbContext = appDbContext;
            _cache = cache;
        }

        public IEnumerable<Country> GetAllCountries()
        {
            if (!_cache.TryGetValue(CountriesKey, out List<Country> allCountries))
            {
                allCountries = _appDbContext.Countries.ToList();
                _cache.Set(CountriesKey, allCountries);
            }
            
            return allCountries;
        }

        public string AddLocations(Dictionary<DateTime, Geometry> locations)
        {
            var guid = Guid.NewGuid().ToString();
            _cache.Set(guid, locations, TimeSpan.FromMinutes(15));
            return guid;
        }
        
        public Dictionary<DateTime, Geometry> GetLocations(string guid)
        {
            return _cache.Get<Dictionary<DateTime, Geometry>>(guid);
        }

        public void SaveUsageHistory(UsageHistory usageHistory)
        {
            _appDbContext.Database.SetCommandTimeout(180);
            _appDbContext.UsageHistory.Add(usageHistory);
            _appDbContext.SaveChanges();
        }
    }
}