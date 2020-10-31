using System;
using System.Globalization;
using NetTopologySuite.Geometries;

namespace BorderCrossing.Models
{
    public class CheckPoint
    {
        public DateTime Date { get; set; }

        public Geometry Point { get; set; }

        public string CountryName { get; set; }

        public string GoogleMapUrl => string.Format("https://www.google.com/maps/search/?api=1&query={0},{1}", this.Point.Coordinate.Y.ToString(CultureInfo.InvariantCulture), this.Point.Coordinate.X.ToString(CultureInfo.InvariantCulture));
    }
}