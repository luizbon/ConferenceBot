using System;
using System.IO;
using System.Web;
using Newtonsoft.Json;

namespace ConferenceBot.Data
{
    public class Messages
    {
        private static string[] FarewellMessages => JsonConvert.DeserializeObject<string[]>(File.ReadAllText(
            HttpContext.Current.Request.MapPath("~\\Data\\FarewellMessages.json")));

        private static string[] GreetingMessages => JsonConvert.DeserializeObject<string[]>(File.ReadAllText(
            HttpContext.Current.Request.MapPath("~\\Data\\GreetingMessages.json")));

        public static string Farewell()
        {
            return FarewellMessages[new Random().Next(0, FarewellMessages.Length)];
        }

        public static string Greetings()
        {
            return GreetingMessages[new Random().Next(0, GreetingMessages.Length)];
        }
    }
}