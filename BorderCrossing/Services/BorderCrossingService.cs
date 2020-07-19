﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Blazor.FileReader;
using BorderCrossing.DbContext;
using BorderCrossing.Models;
using BorderCrossing.Extensions;
using BorderCrossing.Models.Google;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace BorderCrossing.Services
{
    public interface IBorderCrossingService
    {
        Task<DateRangePostRequest> PrepareLocationHistoryAsync(MemoryStream memoryStream);
        Task<BorderCrossingResponse> ParseLocationHistoryAsync(DateRangePostRequest model);
    }

    public class BorderCrossingService : IBorderCrossingService
    {
        private readonly IBorderCrossingRepository _repository;

        public BorderCrossingService(IBorderCrossingRepository repository)
        {
            _repository = repository;
        }

        public async Task<DateRangePostRequest> PrepareLocationHistoryAsync(MemoryStream memoryStream)
        {
            var history = await ExtractJsonAsync(memoryStream);

            if (history != null)
            {
                var locations = PrepareLocations(history);
                var guid = _repository.AddLocations(locations);

                return await Task.FromResult(new DateRangePostRequest()
                {
                    Guid = guid,
                    StartDate = history.Locations.Min(l => l.TimestampMs).ToDateTime(),
                    EndDate = history.Locations.Max(l => l.TimestampMs).ToDateTime()
                });
            }

            return null;
        }

        public async Task<BorderCrossingResponse> ParseLocationHistoryAsync(DateRangePostRequest model)
        {
            var locations = _repository.GetLocations(model.Guid);
            var countries = _repository.GetAllCountries();
            
            var checkPoints =
            (
                from location in locations.Where(l => l.Key >= model.StartDate && l.Key <= model.EndDate).AsParallel().AsOrdered().WithDegreeOfParallelism(10)
                from country in countries.Where(c => location.Value.Within(c.Geom)).DefaultIfEmpty().AsParallel().WithDegreeOfParallelism(10)
                select new CheckPoint()
                {
                    CountryName = country == null ? "Unknown" : country.Name,
                    Date = location.Key,
                    Point = location.Value
                }
            ).AsParallel().WithDegreeOfParallelism(10).OrderBy(p => p.Date).ToList();
            

            var response = new BorderCrossingResponse();
            response.Periods.Add(new Period
            {
                ArrivalPoint = checkPoints.First(),
                Country = checkPoints.First().CountryName,
            });
            var last = response.Periods.Last();
            
            foreach (var place in checkPoints)
            {
                if (place.CountryName == last.Country)
                {
                    continue;
                }
                
                last.DeparturePoint = place;
                response.Periods.Add(new Period
                {
                    ArrivalPoint = place,
                    Country = place.CountryName,
                });
                last = response.Periods.Last();
            }
            
            last.DeparturePoint = checkPoints.Last();

            return await Task.FromResult(response);
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
                if (hour == date.Hour)
                {
                    continue;
                }
                
                hour = date.Hour;
                locations.Add(date, location.Point);
            }

            return locations;
        }
        
        private async Task<LocationHistory> ExtractJsonAsync(MemoryStream memoryStream)
        {
            using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    if (entry.Name == "Location History.json" || entry.Name == "История местоположений.json")
                    {
                        using (var stream = entry.Open())
                        {
                            using (var sr = new StreamReader(stream))
                            using (var reader = new JsonTextReader(sr))
                            {
                                var serializer = new JsonSerializer();
                                return await Task.Run( () => serializer.Deserialize<LocationHistory>(reader));
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}