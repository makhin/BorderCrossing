﻿using System;
using System.Collections.Generic;
using System.Linq;
using BorderCrossing.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Geometries;

namespace BorderCrossing.DbContext
{
    public interface IBorderCrossingRepository
    {
        List<Country> GetAllCountries();
        void SaveLocationHistoryFile(LocationHistoryFile usageHistory);
        string AddLocations(Dictionary<DateTime, Geometry> locations);
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

        public void SaveLocationHistoryFile(LocationHistoryFile usageHistory)
        {
            _appDbContext.Database.SetCommandTimeout(180);
            _appDbContext.UsageHistory.AddAsync(usageHistory);
            _appDbContext.SaveChangesAsync();
        }
    }
}