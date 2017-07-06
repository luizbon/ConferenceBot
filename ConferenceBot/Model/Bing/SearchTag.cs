using Newtonsoft.Json;

namespace ConferenceBot.Model.Bing
{
    public class SearchTag
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("content")]
        public string Content { get; set; }
    }
}