using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using BorderCrossing.Models;
using BorderCrossing.Models.Core;
using BorderCrossing.Models.Google;
using Jil;
using NetTopologySuite.Geometries;

namespace BorderCrossing.Extensions
{
    public static class BorderCrossingHelper
    {
        private static readonly string[] Names = new[]
        {
            "Location History",
            "История местоположений",
            "Historia lokalizacji",
            "Історія місцезнаходжень"
        };

        public static Dictionary<DateTime, Geometry> PrepareLocations(LocationHistory history, IntervalType intervalType)
        {
            var interval = 0;
            var locations = new Dictionary<DateTime, Geometry>();

            foreach (var location in history.Locations)
            {
                if (location?.TimestampMs == null || location.LatitudeE7 == 0 || location.LongitudeE7 == 0)
                {
                    continue;
                }

                var date = location.Date;
                if (intervalType == IntervalType.Day)
                {
                    if (interval == date.Day)
                    {
                        continue;
                    }
                    interval = date.Day;
                }
                else
                {
                    if (interval == date.Hour)
                    {
                        continue;
                    }
                    interval = date.Hour;
                }

                locations.Add(date, location.Point);
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

                    await using (Stream stream = entry.Open())
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

    }
}
