using System;
using BorderCrossing.Extensions;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace BorderCrossing.Models.Google
{
    public class Location
    {
        [JsonProperty("timestampMs")]
        public string TimestampMs { get; set; }

        [JsonProperty("latitudeE7")]
        public long LatitudeE7 { get; set; }

        [JsonProperty("longitudeE7")]
        public long LongitudeE7 { get; set; }

        public Geometry Point
        {
            get
            {
                var latitude = LatitudeE7/1e7;
                var longitude = LongitudeE7/1e7;
                return new Point(longitude, latitude);
            }
        }
        
        public DateTime Date => TimestampMsUnix.ToDateTime();

        public long TimestampMsUnix => long.Parse(this.TimestampMs);

    }
}