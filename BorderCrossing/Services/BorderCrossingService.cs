using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BorderCrossing.DbContext;
using BorderCrossing.Models;
using BorderCrossing.Extensions;
using BorderCrossing.Models.Google;

namespace BorderCrossing.Services
{
    public interface IBorderCrossingService
    {
        Task<DateRangePostRequest> PrepareLocationHistoryAsync(MemoryStream memoryStream, string fileName, ProgressChangedEventHandler callback);
        Task<BorderCrossingResponse> ParseLocationHistoryAsync(DateRangePostRequest model, ProgressChangedEventHandler callback);
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

        public async Task<DateRangePostRequest> PrepareLocationHistoryAsync(MemoryStream memoryStream, string fileName, ProgressChangedEventHandler callback)
        {
            var requestId = Guid.NewGuid();
            _ = _repository.SaveLocationHistoryFileAsync(memoryStream, fileName, requestId);
            LocationHistory locationHistory = await BorderCrossingHelper.ExtractJsonAsync(memoryStream, callback);
            _repository.AddLocationHistory(locationHistory, requestId.ToString());

            return await Task.FromResult(new DateRangePostRequest
            {
                RequestId = requestId.ToString(),
                StartDate = locationHistory.Locations.Min(l => l.TimestampMs).ToDateTime(),
                EndDate = locationHistory.Locations.Max(l => l.TimestampMs).ToDateTime(),
                IntervalType = IntervalType.Day
        });
        }

        public async Task<BorderCrossingResponse> ParseLocationHistoryAsync(DateRangePostRequest model, ProgressChangedEventHandler callback)
        {
            var locationHistory = _repository.GetLocationHistory(model.RequestId);
            var locations = BorderCrossingHelper.PrepareLocations(locationHistory, model.IntervalType);

            var response = new BorderCrossingResponse();

            var checkPoints = locations.Where(l => l.Key >= model.StartDate && l.Key <= model.EndDate).Select(l => new CheckPoint()
            {
                Date = l.Key,
                Point = l.Value
            }).ToList();

            if (!checkPoints.Any())
            {
                return await Task.FromResult(response);
            }

            var arrivalPoint = checkPoints.First();
            var countryName = GetCountryName(arrivalPoint);
            response.Periods.Add(new Period()
            {
                ArrivalPoint = arrivalPoint,
                Country = countryName
            });
            var last = response.Periods.Last();

            int i = 0;
            int count = checkPoints.Count;

            foreach (var checkPoint in checkPoints)
            {
                await Task.Run(() =>
                {
                    i++;
                    callback(this, new ProgressChangedEventArgs((int) (100.0 * i / count), null));
                    countryName = GetCountryName(checkPoint);
                    if (last.Country == countryName)
                    {
                        return;
                    }

                    last.DeparturePoint = checkPoint;

                    response.Periods.Add(new Period
                    {
                        ArrivalPoint = checkPoint,
                        Country = countryName,
                    });
                    last = response.Periods.Last();
                });
            }

            last.DeparturePoint = checkPoints.Last();

            return await Task.FromResult(response);
        }

        private string GetCountryName(CheckPoint checkPoint)
        {
            var country = _countries.FirstOrDefault(c => checkPoint.Point.Within(c.Geom));
            var countryName = country == null ? "Unknown" : country.Name;
            return countryName;
        }
    }
}