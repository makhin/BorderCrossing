using System;
using NetTopologySuite.Geometries;

namespace BorderCrossing.Models
{
    public class CheckPoint
    {
        public DateTime Date { get; set; }
        
        public Geometry Point { get; set; }
        
        public string CountryName { get; set; }
    }
}