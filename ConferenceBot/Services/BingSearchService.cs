using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using ConferenceBot.Data;
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
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _bingSearchApiKey);
            httpClient.DefaultRequestHeaders.Add("X-Search-Location", $"lat:{DDDPerth17.Lat};long:{DDDPerth17.Long};re:100");

            return httpClient;
        }

        public async Task<WebResult> Search(string query)
        {
            using (var client = GetClient())
            {
                var result = await client.GetStringAsync($"{Endpoint}?q={query} loc:Perth&count=10&mkt=en-AU&responseFilter=Webpages&safeSearch=strict");
                return JsonConvert.DeserializeObject<WebResult>(result);
            }
        }
    }
}
