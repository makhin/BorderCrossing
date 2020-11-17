using BorderCrossing.DbContext;
using BorderCrossing.Models;
using BorderCrossing.Models.Core;
using BorderCrossing.Models.Google;
using Jil;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Location = BorderCrossing.Models.Google.Location;

namespace BorderCrossing.Services
{
    public static class BorderCrossingHelper
    {
        private static readonly long MinInterval = (long)new TimeSpan(0, 15, 0).TotalMilliseconds;

        private static readonly string[] Names = new[]
        {
            "Location History",
            "История местоположений",
            "Historia lokalizacji",
            "Історія місцезнаходжень"
        };

        public static List<Location> PrepareLocations(LocationHistory history, IntervalType intervalType)
        {
            var day = 0;
            var hour = 0;
            var locations = new List<Location>();

            foreach (var location in history.Locations)
            {
                if (location?.TimestampMs == null || location.LatitudeE7 == 0 || location.LongitudeE7 == 0)
                {
                    continue;
                }

                var date = location.Date;

                if (intervalType == IntervalType.Day)
                {
                    if (day == date.Day)
                    {
                        continue;
                    }
                }
                else if (intervalType == IntervalType.Every12Hours)
                {
                    if (hour < 12 && day == date.Day && date.Hour < 12)
                    {
                        continue;
                    }

                    if (hour >= 12 && hour < 24 && day == date.Day && date.Hour >= 12 && date.Hour < 24)
                    {
                        continue;
                    }
                }
                else
                {
                    if (hour == date.Hour)
                    {
                        continue;
                    }
                }

                hour = date.Hour;
                day = date.Day;
                locations.Add(location);
            }
            return locations;
        }

        public static async Task<LocationHistory> ExtractJsonAsync(MemoryStream memoryStream, ProgressChangedEventHandler callback)
        {
            using (var zip = new ZipArchive(memoryStream, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    if (Path.GetExtension(entry.Name) != ".json" || !Names.Contains(Path.GetFileNameWithoutExtension(entry.Name)))
                    {
                        continue;
                    }

                    using (Stream stream = entry.Open())
                    {
                        using (ContainerStream containerStream = new ContainerStream(stream))
                        {
                            containerStream.ProgressChanged += callback;

                            using (StreamReader streamReader = new StreamReader(containerStream))
                            {
                                var jilOptions = new Options(excludeNulls: true);
                                return await Task.Run(() => JSON.Deserialize<LocationHistory>(streamReader, jilOptions));
                            }
                        }
                    }
                }
            }

            throw new Exception("Архив не содержит файла с историей местоположений");
        }

        public static CheckPoint FindCheckPoint(List<Location> locations, Location location1, string country1, Location location2, string country2, Func<Geometry, string> getCountry, int callCount = 0)
        {
            callCount++;
            if (locations.Count < 4 || (location2.TimestampMsUnix - location1.TimestampMsUnix) < MinInterval || callCount > 10)
            {
                return LocationToCheckPoint(location2, country2);
            }

            var mid = (long)((location2.TimestampMsUnix + location1.TimestampMsUnix) / 2.0);
            var midLocation = locations.OrderBy(l => Math.Abs(l.TimestampMsUnix - mid)).First();

            if (midLocation == null)
            {
                return LocationToCheckPoint(location2, country2);
            }

            var midCountry = getCountry(midLocation.Point);

            if (midCountry == country2)
            {
                var range = locations.Where(lh => lh.TimestampMsUnix >= location1.TimestampMsUnix && lh.TimestampMsUnix <= midLocation.TimestampMsUnix).ToList();
                return FindCheckPoint(range, location1, country1, midLocation, midCountry, getCountry, callCount);
            }
            else
            {
                var range = locations.Where(lh => lh.TimestampMsUnix >= midLocation.TimestampMsUnix && lh.TimestampMsUnix <= location2.TimestampMsUnix).ToList();
                return FindCheckPoint(range, midLocation, midCountry, location2, country2, getCountry, callCount);
            }
        }

        public static CheckPoint LocationToCheckPoint(Location location, string country)
        {
            return new CheckPoint
            {
                CountryName = country,
                Date = location.Date,
                Point = location.Point
            };
        }
    }
}
