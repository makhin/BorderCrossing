using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace BorderCrossing.DbContext
{
    public class Country
    {
        [Key]
        public string Name { get; set; }
        
        public Geometry Geom { get; set; }
    }
}