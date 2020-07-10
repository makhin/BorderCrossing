using Newtonsoft.Json;

namespace CrossBorders.MVC.Models
{
    public partial class TypeActivity
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("confidence")]
        public long Confidence { get; set; }
    }
}