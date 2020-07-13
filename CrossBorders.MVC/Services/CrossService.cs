using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using CrossBorders.MVC.DbContext;
using CrossBorders.MVC.Extensions;
using CrossBorders.MVC.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace CrossBorders.MVC.Services
{
    public interface ICrossService
    { HistoryStat 
        HistoryProcess(IFormFile modelLocationHistory);

        Models.Results FindCrosses(CalcPost model);
    }

    public class CrossService : ICrossService
    {
        private readonly ICrossRepository _repository;
        private readonly IWebHostEnvironment _environment;

        public CrossService(ICrossRepository repository, IWebHostEnvironment environment)
        {
            _repository = repository;
            _environment = environment;
        }

        public HistoryStat HistoryProcess(IFormFile file)
        {
            
            var usageHistory = new UsageHistory
            {
                DateUpload = DateTime.Now, 
                FileName = file.FileName
            };
            
            History history;
            
            using(var memoryStream = new MemoryStream())
            {
                file.CopyTo(memoryStream);
                usageHistory.File = memoryStream.ToArray();
                //_repository.SaveUsageHistory(usageHistory);
                history = ExtractJson(memoryStream);
            }

            if (history != null)
            {
                var locations = PrepareLocations(history);
                var guid = _repository.AddLocations(locations);
                
                return new HistoryStat
                {
                    Guid = guid,
                    Count = history.Locations.Length,
                    StartDate = history.Locations.Min(l => l.TimestampMs).ToDateTime(),
                    EndDate = history.Locations.Max(l => l.TimestampMs).ToDateTime()
                };
            }

            return null;
        }

        public Models.Results FindCrosses(CalcPost model)
        {
            var locations = _repository.GetLocations(model.Guid);
            var countries = _repository.GetAllCountries();
            
            var places =
            (
                from location in locations.Where(l => l.Key >= model.StartDate && l.Key <= model.EndDate).AsParallel().AsOrdered().WithDegreeOfParallelism(10)
                from country in countries.Where(c => c.Region == 150 && location.Value.Within(c.Geom)).DefaultIfEmpty().AsParallel().WithDegreeOfParallelism(10)
                select new
                {
                    Name = country == null ? "Unknown" : country.Name,
                    Date = location.Key
                }
            ).AsParallel().WithDegreeOfParallelism(10).OrderBy(p => p.Date).ToList();
            

            var result = new Models.Results();
            result.Periods.Add(new Period
            {
                ArrivalDate = places.First().Date,
                Country = places.First().Name,
            });
            var last = result.Periods.Last();
            
            foreach (var place in places)
            {
                if (place.Name == last.Country)
                {
                    continue;
                }
                
                last.DepartureDate = place.Date;
                result.Periods.Add(new Period
                {
                    ArrivalDate = place.Date,
                    Country = place.Name,
                });
                last = result.Periods.Last();
            }
            
            last.DepartureDate = places.Last().Date;
            return result;
        }

        private Dictionary<DateTime, Geometry> PrepareLocations(History history)
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
                if (hour == date.Hour)
                {
                    continue;
                }
                
                hour = date.Hour;
                locations.Add(date, location.Point);
            }

            return locations;
        }
        
        private History ExtractJson(MemoryStream memoryStream)
        {
            using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    if (entry.Name == "Location History.json" || entry.Name == "История местоположений.json")
                    {
                        using (Stream stream = entry.Open())
                        {
                            using (StreamReader sr = new StreamReader(stream))
                            using (JsonReader reader = new JsonTextReader(sr))
                            {
                                JsonSerializer serializer = new JsonSerializer();
                                return serializer.Deserialize<History>(reader);
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}