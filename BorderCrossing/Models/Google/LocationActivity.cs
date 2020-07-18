using Newtonsoft.Json;

namespace BorderCrossing.Models.Google
{
    public partial class LocationActivity
    {
        [JsonProperty("timestampMs")]
        public string TimestampMs { get; set; }

        [JsonProperty("activity")]
        public TypeActivity[] Activity { get; set; }
    }
}