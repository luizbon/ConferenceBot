using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Luis.Models;

namespace ConferenceBot.Extensions
{
    public static class EntityRecommendationExtensions
    {
        public static string GetListResolution(this EntityRecommendation entity)
        {
            var value = (List<object>)entity.Resolution["values"];
            return value.FirstOrDefault()?.ToString();
        }
    }
}