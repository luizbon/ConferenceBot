using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using ConferenceBot.Model.Bing;
using Newtonsoft.Json;

namespace ConferenceBot.Services
{
    public class BindSearchService
    {
        private const string Endpoint = "https://api.cognitive.microsoft.com/bing/v5.0/search";
        private readonly string _bingSearchApiKey;

        public BindSearchService()
        {
            _bingSearchApiKey = ConfigurationManager.AppSettings["BingSearchApiKey"];
        }

        private HttpClient GetClient()
        {
            var httpCliet = new HttpClient();
            httpCliet.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _bingSearchApiKey);

            return httpCliet;
        }

        public async Task<WebResult> Search(string query)
        {
            using (var client = GetClient())
            {
                var result = await client.GetStringAsync($"{Endpoint}?q={query}&count=10&mkt=en-au");
                return JsonConvert.DeserializeObject<WebResult>(result);
            }
        }
    }
}