using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BorderCrossing.Models;
using BorderCrossing.Models.Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Geometries;

namespace BorderCrossing.DbContext
{
    public interface IBorderCrossingRepository
    {
        List<Country> GetAllCountries();
        Task SaveLocationHistoryFileAsync(MemoryStream memoryStream, string fileName, Guid requestId);
        void AddLocationHistory(LocationHistory locationHistory, string requestId);
        LocationHistory GetLocationHistory(string requestId);
        Task<List<CheckPoint>> GetResultAsync(string requestId);
        Task SaveResultAsync(string requestId, List<CheckPoint> response);
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
            if (_cache.TryGetValue(CountriesKey, out List<Country> countries))
            {
                return countries;
            }

            countries = _appDbContext.Countries.Where(c=> c.Region == 150).ToList();
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.NeverRemove);
            _cache.Set(CountriesKey, countries, cacheEntryOptions);

            return countries;
        }

        public void AddLocationHistory(LocationHistory locationHistory, string requestId)
        {
            _cache.Set(requestId, locationHistory, TimeSpan.FromMinutes(15));
        }
        
        public LocationHistory GetLocationHistory(string requestId)
        {
            return _cache.Get<LocationHistory>(requestId);
        }

        public Task<List<CheckPoint>> GetResultAsync(string requestId)
        {
            throw new NotImplementedException();
        }

        public Task SaveResultAsync(string requestId, List<CheckPoint> response)
        {
            throw new NotImplementedException();
        }

        public async Task SaveLocationHistoryFileAsync(MemoryStream memoryStream, string fileName, Guid requestId)
        {
            _appDbContext.Database.SetCommandTimeout(180);
            var locationHistoryFile = new LocationHistoryFile
            {
                File = memoryStream.ToArray(),
                DateUpload = DateTime.Now,
                RequestId = requestId,
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