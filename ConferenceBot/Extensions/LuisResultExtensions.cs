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
        public static bool TryFindTime(this LuisResult result, string timeFilter, string dateFilter, string nextFilter, out DateTime start, out DateTime end)
        {
            start = DateTime.MinValue;
            end = DateTime.MinValue;
            if (result.TryFindEntity(nextFilter, out EntityRecommendation dateEntity))
            {
                var currentDateTime = TimeZoneInfo
                    .ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"));

                var firstOrDefault = NdcSydney17.Data.Timeslots
                    .FirstOrDefault(x => x.Date >= currentDateTime);

                start = firstOrDefault?.Date ?? currentDateTime;
                end = firstOrDefault?.Date ?? currentDateTime.AddHours(2);
                return true;
            }

            if (!result.TryFindEntity(timeFilter, out dateEntity))
                if (!result.TryFindEntity(dateFilter, out dateEntity))
                    return false;

            var parser = new Parser();

            var parsedDateTime = parser.Parse(dateEntity.Entity);

            if (parsedDateTime == null) return false;

            if (parsedDateTime.Start != null) start = parsedDateTime.Start.Value;
            if (parsedDateTime.End != null) end = parsedDateTime.End.Value;
            return true;
        }
    }
}