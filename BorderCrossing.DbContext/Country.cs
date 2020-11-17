using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BorderCrossing.DbContext
{
    public class Country
    {
        [Key]
        public string Name { get; set; }

        public short Region { get; set; }

        [Column(TypeName = "geometry")]
        public Geometry Geom { get; set; }
    }
}