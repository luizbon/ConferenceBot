using Newtonsoft.Json;

namespace ConferenceBot.Model.Bing
{
    public class WebResult
    {
        [JsonProperty("_type")]
        public string Type { get; set; }
        [JsonProperty("webPages")]
        public WebPages WebPages { get; set; }
    }
}