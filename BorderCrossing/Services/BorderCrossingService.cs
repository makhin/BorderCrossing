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
using BorderCrossing.Models.Core;
using BorderCrossing.Models.Google;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

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
        private static readonly string[] Names = new []
        {
            "Location History",
            "История местоположений",
            "Historia lokalizacji",
            "Історія місцезнаходжень"
        };

        public BorderCrossingService(IBorderCrossingRepository repository)
        {
            _repository = repository;
            _countries = _repository.GetAllCountries();
        }

        public async Task<DateRangePostRequest> PrepareLocationHistoryAsync(MemoryStream memoryStream, string fileName, ProgressChangedEventHandler callback)
        {
            var guid = Guid.NewGuid();
            _ = _repository.SaveLocationHistoryFileAsync(memoryStream, fileName, guid);

            var history = await ExtractJsonAsync(memoryStream, callback);
            var locations = PrepareLocations(history);
            _repository.AddLocations(locations, guid.ToString());

            return await Task.FromResult(new DateRangePostRequest
            {
                Guid = guid.ToString(),
                StartDate = history.Locations.Min(l => l.TimestampMs).ToDateTime(),
                EndDate = history.Locations.Max(l => l.TimestampMs).ToDateTime(),
                Interval = 1
        });
        }

        public async Task<BorderCrossingResponse> ParseLocationHistoryAsync(DateRangePostRequest model, ProgressChangedEventHandler callback)
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
                    if (Path.GetExtension(entry.Name) != ".json" || !Names.Contains(Path.GetFileNameWithoutExtension(entry.Name)))
                    {
                        continue;
                    }

                    await using (Stream stream = entry.Open())
                    {
                        using (ContainerStream containerStream = new ContainerStream(stream))
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

            throw new Exception("Архив не содержит файла с историей местоположений");
        }
    }
}