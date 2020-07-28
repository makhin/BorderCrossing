using System;
using System.Runtime.Serialization;
using BorderCrossing.Extensions;
using BorderCrossing.Models.Core;
using Jil;
using NetTopologySuite.Geometries;

namespace BorderCrossing.Models.Google
{
    public class Location
    {
        [JilDirective(Name = "timestampMs")]
        public string TimestampMs { get; set; }

        [JilDirective(Name = "latitudeE7")]
        public long LatitudeE7 { get; set; }

        [JilDirective(Name = "longitudeE7")]
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
        
        public DateTime Date => TimestampMs.ToDateTime();
    }
}