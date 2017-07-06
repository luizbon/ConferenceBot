using Newtonsoft.Json;

namespace ConferenceBot.Model.Bing
{
    public class DeepLink
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("snippet")]
        public string Snippet { get; set; }
    }
}