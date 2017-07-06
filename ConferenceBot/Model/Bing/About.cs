using Newtonsoft.Json;

namespace ConferenceBot.Model.Bing
{
    public class About
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}