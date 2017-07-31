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

                var firstOrDefault = NdcSydney17.Data.Timeslots
                    .FirstOrDefault(x => x.Date.TimeOfDay >= currentTimeSpan);

                time = firstOrDefault?.Date.TimeOfDay ?? currentTimeSpan;
                return true;
            }

            if (!result.TryFindEntity(timeFilter, out timeEntity)) return false;

            var parser = new Parser();

            var dateTime = parser.Parse(timeEntity.Entity).Start;

            if (dateTime == null) return false;

            time = dateTime.Value.TimeOfDay;
            return true;
        }

        public static bool TryFindDate(this LuisResult result, string dateFilter, out DateTime startDate,
            out DateTime endDate)
        {
            startDate = DateTime.MinValue;
            endDate = DateTime.MinValue;

            if (!result.TryFindEntity(dateFilter, out EntityRecommendation dateEntity))
                return false;
            
            var options = new Options
            {
                Clock = () => NdcSydney17.Data.Timeslots.Min(t => t.Date.Date).AddDays(-1)
            };

            var parser = new Parser(options);

            var dateTime = parser.Parse(dateEntity.Entity);

            if (dateTime.Start != null) startDate = dateTime.Start.Value;
            if (dateTime.End != null) endDate = dateTime.End.Value;

            return true;
        }
    }
}