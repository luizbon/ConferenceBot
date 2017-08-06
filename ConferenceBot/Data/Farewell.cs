using System;
using System.IO;
using System.Web;
using Newtonsoft.Json;

namespace ConferenceBot.Data
{
    public class Farewell
    {
        private static string[] Data => JsonConvert.DeserializeObject<string[]>(File.ReadAllText(
            HttpContext.Current.Request.MapPath("~\\Data\\FarewellMessages.json")));

        public static string Random()
        {
            return Data[new Random().Next(0, Data.Length)];
        }
    }
}