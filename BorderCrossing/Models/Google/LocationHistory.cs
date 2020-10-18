using Jil;

namespace BorderCrossing.Models.Google
{
    public partial class LocationHistory
    {
        [JilDirective(Name = "locations")]
        public Location[] Locations { get; set; }
    }
}