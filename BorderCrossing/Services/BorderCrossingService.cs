using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using BorderCrossing.Extensions;
using BorderCrossing.Models;
using BorderCrossing.Models.Google;
using NetTopologySuite.Geometries;

namespace BorderCrossing.Services
{
    public interface IBorderCrossingService
    {
        Task<QueryRequest> GetQueryRequestAsync(LocationHistory locationHistory);
        Task<List<CheckPoint>> ParseLocationHistoryAsync(LocationHistory locationHistory, QueryRequest model, ProgressChangedEventHandler callback);
    }

    public class BorderCrossingService : IBorderCrossingService
    {
        public async Task<QueryRequest> GetQueryRequestAsync(LocationHistory locationHistory)
        {
            return await Task.FromResult(new QueryRequest
            {
                StartDate = locationHistory.Locations.Min(l => l.TimestampMsUnix).ToDateTime(),
                EndDate = locationHistory.Locations.Max(l => l.TimestampMsUnix).ToDateTime(),
                IntervalType = IntervalType.Every12Hours
            });
        }

        public async Task<List<CheckPoint>> ParseLocationHistoryAsync(LocationHistory locationHistory, QueryRequest model, ProgressChangedEventHandler callback)
        {
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

            return checkPoints;
        }

        private string GetCountryName(Geometry point)
        {
            var country = CountryStorage.Countries.FirstOrDefault(c => point.Within(c.Geom));

            country ??= CountryStorage.Countries
                .Select(c => new { Country = c, Distance = c.Geom.Distance(point) })
                .OrderBy(d => d.Distance)
                .FirstOrDefault(c => c.Distance * 100 < 10)?.Country;

            return country == null ? "Unknown" : country.Name;
        }
    }
}