using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using ConferenceBot.Model.QnA;
using Newtonsoft.Json;

namespace ConferenceBot.Services
{
    public class QnAService
    {
        private readonly Uri _baseUri = new Uri("https://ddd-sydney-18-qna.azurewebsites.net/qnamaker");
        private readonly UriBuilder _builder;
        private readonly string _qnamakerSubscriptionKey;

        public QnAService() : this(ConfigurationManager.AppSettings["KnowledgeBaseId"])
        {
        }

        public QnAService(string knowledgebaseId)
        {
            _qnamakerSubscriptionKey = ConfigurationManager.AppSettings["QnASubscriptionKey"];
            _builder = new UriBuilder($"{_baseUri}/knowledgebases/{knowledgebaseId}/generateAnswer");
        }

        private HttpClient GetClient()
        {
            var httpCliet = new HttpClient();
            httpCliet.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _qnamakerSubscriptionKey);

            return httpCliet;
        }

        public async Task<QnAMakerResult> Search(string query)
        {
            using (var client = GetClient())
            {
                var response = await client.PostAsJsonAsync(_builder.Uri, new
                {
                    question = query
                });

                if (!response.IsSuccessStatusCode)
                    return null;

                var result = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<QnAMakerResult>(result);
            }
        }
    }
}