using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using BorderCrossing.DbContext;
using BorderCrossing.Models;
using BorderCrossing.Extensions;
using BorderCrossing.Models.Google;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace BorderCrossing.Services
{
    public interface IBorderCrossingService
    {
        Task<DateRangePostRequest> PrepareLocationHistoryAsync(MemoryStream memoryStream, ProgressChangedEventHandler callback);
        BorderCrossingResponse ParseLocationHistory(DateRangePostRequest model, ProgressChangedEventHandler callback);
    }

    public class BorderCrossingService : IBorderCrossingService
    {
        private readonly IBorderCrossingRepository _repository;
        private readonly List<Country> _countries;

        public BorderCrossingService(IBorderCrossingRepository repository)
        {
            _repository = repository;
            _countries = _repository.GetAllCountries().ToList();
        }

        public async Task<DateRangePostRequest> PrepareLocationHistoryAsync(MemoryStream memoryStream, ProgressChangedEventHandler callback)
        {
            var history = await ExtractJsonAsync(memoryStream, callback);

            if (history != null)
            {
                var locations = PrepareLocations(history);
                var guid = _repository.AddLocations(locations);

                return await Task.FromResult(new DateRangePostRequest
                {
                    Guid = guid,
                    StartDate = history.Locations.Min(l => l.TimestampMs).ToDateTime(),
                    EndDate = history.Locations.Max(l => l.TimestampMs).ToDateTime()
                });
            }

            return null;
        }

        public BorderCrossingResponse ParseLocationHistory(DateRangePostRequest model, ProgressChangedEventHandler callback)
        {
            Dictionary<DateTime, Geometry> locations = _repository.GetLocations(model.Guid);
            var response = new BorderCrossingResponse();

            var checkPoints = locations.Where(l => l.Key >= model.StartDate && l.Key <= model.EndDate).Select(l => new CheckPoint()
            {
                Date = l.Key,
                Point = l.Value
            }).ToList();

            if (!checkPoints.Any())
            {
                return response;
            }

            Period last = null;
            var arrivalPoint = checkPoints.First();
            var countryName = GetCountryName(arrivalPoint);
            response.Periods.Add(new Period()
            {
                ArrivalPoint = arrivalPoint,
                Country = countryName
            });
            last = response.Periods.Last();

            int i = 0;
            int count = checkPoints.Count;

            foreach (var checkPoint in checkPoints)
            {
                i++;
                callback(this, new ProgressChangedEventArgs( (int)(100.0 * i / count), null));
                countryName = GetCountryName(checkPoint);
                if (last.Country == countryName)
                {
                    continue;
                }

                last.DeparturePoint = checkPoint;

                response.Periods.Add(new Period
                {
                    ArrivalPoint = checkPoint,
                    Country = countryName,
                });
                last = response.Periods.Last();
            }
            last.DeparturePoint = checkPoints.Last();

            return response;
        }

        private string GetCountryName(CheckPoint checkPoint)
        {
            var country = _countries.FirstOrDefault(c => checkPoint.Point.Within(c.Geom));
            var countryName = country == null ? "Unknown" : country.Name;
            return countryName;
        }

        private static CheckPoint GetCheckpoint(string countryName, KeyValuePair<DateTime, Geometry> location)
        {
            return new CheckPoint()
            {
                Date = location.Key,
                Point = location.Value
            };
        }

        private Dictionary<DateTime, Geometry> PrepareLocations(LocationHistory history)
        {
            var hour = 0;
            var locations = new Dictionary<DateTime, Geometry>();

            foreach (var location in history.Locations)
            {
                if (location?.TimestampMs == null || location.LatitudeE7 == 0 || location.LongitudeE7 == 0)
                {
                    continue;
                }
                
                var date = location.Date;
                if (hour == date.Day)
                {
                    continue;
                }
                
                hour = date.Day;
                locations.Add(date, location.Point);
            }

            return locations;
        }
        
        private async Task<LocationHistory> ExtractJsonAsync(MemoryStream memoryStream, ProgressChangedEventHandler callback)
        {
            using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Read))
            {

                foreach (var entry in zip.Entries)
                {
                    if (entry.Name != "Location History.json" && entry.Name != "История местоположений.json")
                    {
                        continue;
                    }

                    await using (Stream stream = entry.Open())
                    {
                        using (ContainerStream containerStream = new ContainerStream(stream as DeflateStream))
                        {
                            containerStream.ProgressChanged += callback;

                            using (StreamReader streamReader = new StreamReader(containerStream))
                            using (JsonTextReader jsonTextReader = new JsonTextReader(streamReader))
                            {
                                var serializer = new JsonSerializer();
                                return await Task.Run(() => serializer.Deserialize<LocationHistory>(jsonTextReader));
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}