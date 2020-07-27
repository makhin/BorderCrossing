using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BorderCrossing.DbContext;
using BorderCrossing.Models;
using BorderCrossing.Extensions;
using NetTopologySuite.Geometries;

namespace BorderCrossing.Services
{
    public interface IBorderCrossingService
    {
        Task<string> PrepareLocationHistoryAsync(MemoryStream memoryStream, string fileName, ProgressChangedEventHandler callback);
        Task<QueryRequest> GetQueryRequestAsync(string requestId);
        Task ParseLocationHistoryAsync(string requestId, QueryRequest model, ProgressChangedEventHandler callback);
        Task<List<CheckPoint>> GetResultAsync(string requestId);
    }

    public class BorderCrossingService : IBorderCrossingService
    {
        private readonly IBorderCrossingRepository _repository;
        private readonly List<Country> _countries;

        public BorderCrossingService(IBorderCrossingRepository repository)
        {
            _repository = repository;
            _countries = _repository.GetAllCountries();
        }

        public async Task<string> PrepareLocationHistoryAsync(MemoryStream memoryStream, string fileName, ProgressChangedEventHandler callback)
        {
            var requestId = Guid.NewGuid();
            _ = _repository.SaveLocationHistoryFileAsync(memoryStream, fileName, requestId);
            var locationHistory = await BorderCrossingHelper.ExtractJsonAsync(memoryStream, callback);
            _repository.AddLocationHistory(locationHistory, requestId.ToString());
            return requestId.ToString();
        }

        public async Task<QueryRequest> GetQueryRequestAsync(string requestId)
        {
            var locationHistory = _repository.GetLocationHistory(requestId);

            return await Task.FromResult(new QueryRequest
            {
                StartDate = locationHistory.Locations.Min(l => l.TimestampMs).ToDateTime(),
                EndDate = locationHistory.Locations.Max(l => l.TimestampMs).ToDateTime(),
                IntervalType = IntervalType.Day
            });
        }

        public async Task ParseLocationHistoryAsync(string requestId, QueryRequest model, ProgressChangedEventHandler callback)
        {
            var locationHistory = _repository.GetLocationHistory(requestId);
            var locations = BorderCrossingHelper.PrepareLocations(locationHistory, model.IntervalType);
            var filteredLocations = locations.Where(l => l.Key >= model.StartDate && l.Key <= model.EndDate).ToList();

            var checkPoints = new List<CheckPoint>();

            var currentCountry = string.Empty;
            int i = 0;
            int count = filteredLocations.Count();

            foreach (var (date, point) in filteredLocations)
            {
                await Task.Run(() =>
                {
                    i++;
                    callback(this, new ProgressChangedEventArgs((int)(100.0 * i / count), null));

                    var countryName = GetCountryName(point);
                    if (currentCountry != countryName)
                    {
                        checkPoints.Add(new CheckPoint()
                        {
                            CountryName = countryName,
                            Date = date,
                            Point = point
                        });
                    }
                });
            }

            var last = filteredLocations.Last();
            checkPoints.Add(new CheckPoint()
            {
                CountryName = GetCountryName(last.Value),
                Point = last.Value,
                Date = last.Key
            });

            await _repository.SaveResultAsync(requestId, checkPoints);
        }

        public async Task<List<CheckPoint>> GetResultAsync(string requestId)
        {
            return await _repository.GetResultAsync(requestId);
        }

        private string GetCountryName(Geometry location)
        {
            var country = _countries.FirstOrDefault(c => location.Within(c.Geom));
            var countryName = country == null ? "Unknown" : country.Name;
            return countryName;
        }
    }
}