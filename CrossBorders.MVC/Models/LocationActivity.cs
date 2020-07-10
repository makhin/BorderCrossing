using Newtonsoft.Json;

namespace CrossBorders.MVC.Models
{
    public partial class LocationActivity
    {
        [JsonProperty("timestampMs")]
        public string TimestampMs { get; set; }

        [JsonProperty("activity")]
        public TypeActivity[] Activity { get; set; }
    }
}