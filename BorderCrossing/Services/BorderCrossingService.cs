using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BorderCrossing.DbContext;
using BorderCrossing.Models;
using BorderCrossing.Extensions;
using Microsoft.Extensions.Primitives;
using NetTopologySuite.Geometries;

namespace BorderCrossing.Services
{
    public interface IBorderCrossingService
    {
        Task<string> PrepareLocationHistoryAsync(MemoryStream memoryStream, string fileName, Request request, ProgressChangedEventHandler callback);
        Task<QueryRequest> GetQueryRequestAsync(string requestId);
        Task ParseLocationHistoryAsync(string requestId, QueryRequest model, ProgressChangedEventHandler callback);
        Task<List<CheckPoint>> GetResultAsync(string requestId);
        Task<Request> AddNewRequestAsync(string ipAddress, string userAgent);
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

        public async Task<string> PrepareLocationHistoryAsync(MemoryStream memoryStream, string fileName, Request request, ProgressChangedEventHandler callback)
        {
            _ = _repository.SaveLocationHistoryFileAsync(memoryStream, fileName, request);
            var locationHistory = await BorderCrossingHelper.ExtractJsonAsync(memoryStream, callback);
            _repository.AddLocationHistory(locationHistory, request.RequestId.ToString());
            return request.RequestId.ToString();
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

            foreach (var (date, point) in filteredLocations.OrderBy(l => l.Key))
            {
                await Task.Run(() =>
                {
                    i++;
                    callback(this, new ProgressChangedEventArgs((int)(100.0 * i / count), null));

                    var countryName = GetCountryName(point);
                    if (currentCountry == countryName)
                    {
                        return;
                    }

                    checkPoints.Add(new CheckPoint()
                    {
                        CountryName = countryName,
                        Date = date,
                        Point = point
                    });
                    currentCountry = countryName;
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

        public Task<Request> AddNewRequestAsync(string ipAddress, string userAgent)
        {
            return _repository.AddNewRequest(Guid.NewGuid(), ipAddress, userAgent);
        }

        private string GetCountryName(Geometry point)
        {
            var country = _countries.FirstOrDefault(c => point.Within(c.Geom)) ?? _countries
                .Select(c => new {Country = c, Distance = c.Geom.Distance(point),})
                .OrderBy(d => d.Distance).FirstOrDefault()?.Country;

            return country == null ? "Unknown" : country.Name;
        }
    }
}