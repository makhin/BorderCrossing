using NetTopologySuite.Geometries;

namespace BorderCrossing.Models
{
    public class Country
    {
        public string Name { get; set; }

        public short Region { get; set; }

        public Geometry Geom { get; set; }
    }
}