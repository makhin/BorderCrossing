using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BorderCrossing.DbContext;
using BorderCrossing.Extensions;
using BorderCrossing.Models;
using BorderCrossing.Models.Google;
using Microsoft.Extensions.Caching.Memory;
using NetTopologySuite.Geometries;

namespace BorderCrossing.Services
{
    public interface IBorderCrossingService
    {
        Task PrepareLocationHistoryAsync(MemoryStream memoryStream, string fileName, string requestId, ProgressChangedEventHandler callback);
        Task<QueryRequest> GetQueryRequestAsync(string requestId);
        Task ParseLocationHistoryAsync(string requestId, QueryRequest model, ProgressChangedEventHandler callback);
        Task<List<CheckPoint>> GetResultAsync(string requestId);
        Task<string> AddNewRequestAsync(string ipAddress, string userAgent);
    }

    public class BorderCrossingService : IBorderCrossingService
    {
        private readonly IBorderCrossingRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly List<Country> _countries;

        public BorderCrossingService(IBorderCrossingRepository repository, IMemoryCache cache)
        {
            _repository = repository;
            _cache = cache;
            _countries = _repository.GetAllCountries();
        }

        public async Task PrepareLocationHistoryAsync(MemoryStream memoryStream, string fileName, string requestId, ProgressChangedEventHandler callback)
        {
            var locationHistory = await BorderCrossingHelper.ExtractJsonAsync(memoryStream, callback);
            AddLocationHistory(locationHistory, requestId);
        }

        public async Task<QueryRequest> GetQueryRequestAsync(string requestId)
        {
            var locationHistory = GetLocationHistory(requestId);

            return await Task.FromResult(new QueryRequest
            {
                StartDate = locationHistory.Locations.Min(l => l.TimestampMsUnix).ToDateTime(),
                EndDate = locationHistory.Locations.Max(l => l.TimestampMsUnix).ToDateTime(),
                IntervalType = IntervalType.Every12Hours
            });
        }

        public async Task ParseLocationHistoryAsync(string requestId, QueryRequest model, ProgressChangedEventHandler callback)
        {
            var locationHistory = GetLocationHistory(requestId);
            var locations = BorderCrossingHelper.PrepareLocations(locationHistory, model.IntervalType);
            var filteredLocations = locations.Where(l => l.Date >= model.StartDate && l.Date <= model.EndDate).OrderBy(l => l.TimestampMsUnix).ToList();
            
            var currentLocation = filteredLocations.First();
            var currentCountry = GetCountryName(currentLocation.Point);
            
            var i = 0;
            var count = filteredLocations.Count();

            var checkPoints = new List<CheckPoint>
            {
                BorderCrossingHelper.LocationToCheckPoint(currentLocation, currentCountry)
            };

            foreach (var location in filteredLocations)
            {
                await Task.Run(() =>
                {
                    i++;
                    callback(this, new ProgressChangedEventArgs((int)(100.0 * i / count), null));

                    var countryName = GetCountryName(location.Point);
                    if (currentCountry == countryName)
                    {
                        currentLocation = location;
                        return;
                    }

                    var range = locationHistory.Locations.Where(lh => lh.TimestampMsUnix >= currentLocation.TimestampMsUnix && lh.TimestampMsUnix <= location.TimestampMsUnix).ToList();
                    var checkPoint = BorderCrossingHelper.FindCheckPoint(range, currentLocation, currentCountry, location, countryName, GetCountryName);

                    checkPoints.Add(checkPoint);
                    currentCountry = countryName;
                    currentLocation = location;
                });
            }

            var last = filteredLocations.Last();
            checkPoints.Add(BorderCrossingHelper.LocationToCheckPoint(last, GetCountryName(last.Point)));

            await _repository.UpdateResultAsync(requestId, checkPoints);
        }


        public async Task<List<CheckPoint>> GetResultAsync(string requestId)
        {
            return await _repository.GetResultAsync(requestId);
        }

        public async Task<string> AddNewRequestAsync(string ipAddress, string userAgent)
        {
            var request = await _repository.AddNewRequest(Guid.NewGuid(), ipAddress, userAgent);
            return request.RequestId.ToString();
        }

        private string GetCountryName(Geometry point)
        {
            var country = _countries.FirstOrDefault(c => point.Within(c.Geom));

            country ??= _countries
                .Select(c => new {Country = c, Distance = c.Geom.Distance(point)})
                .OrderBy(d => d.Distance)
                .FirstOrDefault(c => c.Distance * 100 < 10)?.Country;

            return country == null ? "Unknown" : country.Name;
        }

        private void AddLocationHistory(LocationHistory locationHistory, string requestId)
        {
            _cache.Set(requestId, locationHistory, TimeSpan.FromMinutes(15));
        }

        private LocationHistory GetLocationHistory(string requestId)
        {
            return _cache.Get<LocationHistory>(requestId);
        }
    }
}