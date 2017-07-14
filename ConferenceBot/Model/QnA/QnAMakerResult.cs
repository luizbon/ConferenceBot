using Newtonsoft.Json;

namespace ConferenceBot.Model.QnA
{
    public class QnAMakerResult
    {
        [JsonProperty(PropertyName = "answer")]
        public string Answer { get; set; }

        [JsonProperty(PropertyName = "score")]
        public double Score { get; set; }
    }
}