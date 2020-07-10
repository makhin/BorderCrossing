using Newtonsoft.Json;

namespace CrossBorders.MVC.Models
{
    public partial class History
    {
        [JsonProperty("locations")]
        public Location[] Locations { get; set; }
    }
}