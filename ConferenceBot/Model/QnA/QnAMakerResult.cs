using System.Collections.Generic;
using Newtonsoft.Json;

namespace ConferenceBot.Model.QnA
{
    public class Metadata
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }

    public class AnswerResult
    {
        [JsonProperty(PropertyName = "questions")]
        public IList<string> Questions { get; set; }
        [JsonProperty(PropertyName = "answer")]
        public string Answer { get; set; }
        [JsonProperty(PropertyName = "score")]
        public double Score { get; set; }
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "source")]
        public string Source { get; set; }
        [JsonProperty(PropertyName = "keywords")]
        public IList<object> Keywords { get; set; }
        [JsonProperty(PropertyName = "metadata")]
        public IList<Metadata> Metadata { get; set; }
    }

    public class QnAAnswer
    {
        [JsonProperty(PropertyName = "answers")]
        public IList<AnswerResult> Answers { get; set; }
    }
}