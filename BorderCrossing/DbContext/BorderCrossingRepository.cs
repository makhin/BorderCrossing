using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BorderCrossing.Models.Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace BorderCrossing.DbContext
{
    public interface IBorderCrossingRepository
    {
        List<Country> GetAllCountries();
        Task SaveLocationHistoryFileAsync(MemoryStream memoryStream, string fileName, Request request);
        void AddLocationHistory(LocationHistory locationHistory, string requestId);
        LocationHistory GetLocationHistory(string requestId);
        Task<List<CheckPoint>> GetResultAsync(string requestId);
        Task SaveResultAsync(string requestId, List<CheckPoint> checkPoints);
        Task<Request> AddNewRequest(Guid newGuid, string ipAddress, string userAgent);
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

        public async Task<List<CheckPoint>> GetResultAsync(string requestId)
        {
            return await _appDbContext.CheckPoints.Where(cp => cp.Request.RequestId == Guid.Parse(requestId)).OrderBy(cp => cp.Date).ToListAsync();
        }

        public async Task SaveResultAsync(string requestId, List<CheckPoint> checkPoints)
        {
            var request = await _appDbContext.Requests.FindAsync(Guid.Parse(requestId));
            
            foreach (var checkPoint in checkPoints)
            {
                checkPoint.Request = request;
            }

            await _appDbContext.CheckPoints.AddRangeAsync(checkPoints);

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

        public async Task<Request> AddNewRequest(Guid guid, string ipAddress, string userAgent)
        {
            var request = new Request()
            {
                RequestId = guid,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };

            await _appDbContext.Requests.AddAsync(request);

            try
            {
                await _appDbContext.SaveChangesAsync();
                return request;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }

        public async Task SaveLocationHistoryFileAsync(MemoryStream memoryStream, string fileName, Request request)
        {
            _appDbContext.Database.SetCommandTimeout(180);
            var locationHistoryFile = new LocationHistoryFile
            {
                File = memoryStream.ToArray(),
                DateUpload = DateTime.Now,
                FileName = fileName,
                Request = request
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