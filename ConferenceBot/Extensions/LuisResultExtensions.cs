using System;
using System.Linq;
using Chronic;
using ConferenceBot.Data;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Newtonsoft.Json.Linq;

namespace ConferenceBot.Extensions
{
    public static class LuisResultExtensions
    {
        public static bool TryFindTime(this LuisResult result, string timeFilter, string nextFilter, out TimeSpan time, out bool isNext)
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
                isNext = true;
                return true;
            }

            isNext = false;

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
            
            var today = TimeZoneInfo
                .ConvertTime(DateTime.Now, TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"))
                .Date;

            var options = new Options
            {
                Clock = () => NdcSydney17.Data.Timeslots.Where(t => t.Date.Date >= today).Min(t => t.Date.Date)
            };

            var parser = new Parser(options);

            var dateTime = parser.Parse(dateEntity.Entity);

            if (dateTime.Start != null) startDate = dateTime.Start.Value;
            if (dateTime.End != null) endDate = dateTime.End.Value;

            return true;
        }

        public static bool TryFindDateTime(this LuisResult result, string dateTimeFilter, out DateTime dateTime)
        {
            dateTime = DateTime.MinValue;
            
            if (!result.TryFindEntity(dateTimeFilter, out EntityRecommendation dateEntity))
                return false;

            var value = (string)JArray.Parse(dateEntity.Resolution["values"].ToString())[0]["value"];

            dateTime = TimeZoneInfo
                .ConvertTime(DateTime.Parse(value), TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time"));

            return true;
        }
    }
}