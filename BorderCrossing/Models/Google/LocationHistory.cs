using Newtonsoft.Json;

namespace BorderCrossing.Models.Google
{
    public partial class LocationHistory
    {
        [JsonProperty("locations")]
        public Location[] Locations { get; set; }
    }
}