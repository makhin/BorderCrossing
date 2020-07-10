using System;
using CrossBorders.MVC.Extensions;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace CrossBorders.MVC.Models
{
    public partial class Location
    {
        [JsonProperty("timestampMs")]
        public double TimestampMs { get; set; }

        [JsonProperty("latitudeE7")]
        public long LatitudeE7 { get; set; }

        [JsonProperty("longitudeE7")]
        public long LongitudeE7 { get; set; }

        [JsonProperty("accuracy")]
        public long Accuracy { get; set; }

        [JsonProperty("activity")]
        public LocationActivity[] Activity { get; set; }

        public Geometry Point
        {
            get
            {
                var latitude = LatitudeE7/1e7;
                var longitude = LongitudeE7/1e7;
                return new Point(latitude, longitude);
            }
        }

        public DateTime Date => TimestampMs.ToDateTime();
    }
}