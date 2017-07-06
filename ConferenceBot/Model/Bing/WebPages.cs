using System.Collections.Generic;
using Newtonsoft.Json;

namespace ConferenceBot.Model.Bing
{
    public class WebPages
    {
        [JsonProperty("webSearchUrl")]
        public string WebSearchUrl { get; set; }
        [JsonProperty("totalEstimatedMatches")]
        public int TotalEstimatedMatches { get; set; }
        [JsonProperty("value")]
        public List<Value> Value { get; set; }
    }
}