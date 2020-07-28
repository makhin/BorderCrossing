using System;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace BorderCrossing.DbContext
{
    public class CheckPoint
    {
        public int CheckPointId { get; set; }

        public Request Request { get; set; }

        public DateTime Date { get; set; }

        [Column(TypeName = "geometry")]
        public Geometry Point { get; set; }

        public string CountryName { get; set; }
    }
}