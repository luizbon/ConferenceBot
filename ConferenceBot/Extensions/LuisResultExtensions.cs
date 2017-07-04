using System;
using System.Linq;
using Chronic;
using ConferenceBot.Data;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace ConferenceBot.Extensions
{
    public static class LuisResultExtensions
    {
        public static bool TryFindTime(this LuisResult result, string timeFilter, string nextFilter, out TimeSpan time)
        {
            time = TimeSpan.Zero;
            if (result.TryFindEntity(nextFilter, out EntityRecommendation timeEntity))
            {
                var currentTimeSpan = TimeZoneInfo
                    .ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"))
                    .TimeOfDay;

                var firstOrDefault = DDDSydney17.Data.Timeslots
                    .FirstOrDefault(x => x.Time >= currentTimeSpan);

                time = firstOrDefault?.Time ?? currentTimeSpan;
                return true;
            }

            if (!result.TryFindEntity(timeFilter, out timeEntity)) return false;

            var parser = new Parser();

            var dateTime = parser.Parse(timeEntity.Entity).Start;

            if (dateTime == null) return false;

            time = dateTime.Value.TimeOfDay;
            return true;
        }
    }
}