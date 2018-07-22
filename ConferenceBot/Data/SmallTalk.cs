using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using ConferenceBot.Model;
using Newtonsoft.Json;

namespace ConferenceBot.Data
{
    public static class SmallTalk
    {
        public static string QnAMakerSubscriptionId = ConfigurationManager.AppSettings["SmallTalkId"];
        public static Dictionary<string, string[]> Data => JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText(
            HttpContext.Current.Request.MapPath("~\\Data\\smalltalk.json")));
    }
}