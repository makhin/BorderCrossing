using Newtonsoft.Json;

namespace BorderCrossing.Models.Google
{
    public partial class TypeActivity
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("confidence")]
        public long Confidence { get; set; }
    }
}