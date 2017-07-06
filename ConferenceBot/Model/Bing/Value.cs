using System.Collections.Generic;
using Newtonsoft.Json;

namespace ConferenceBot.Model.Bing
{
    public class Value
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("about")]
        public List<About> About { get; set; }
        [JsonProperty("displayUrl")]
        public string DisplayUrl { get; set; }
        [JsonProperty("snippet")]
        public string Snippet { get; set; }
        [JsonProperty("deepLinks")]
        public List<DeepLink> DeepLinks { get; set; }
        [JsonProperty("dateLastCrawled")]
        public string DateLastCrawled { get; set; }
        [JsonProperty("searchTags")]
        public List<SearchTag> SearchTags { get; set; }
    }
}