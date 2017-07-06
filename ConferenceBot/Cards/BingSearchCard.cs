using System.Collections.Generic;
using System.Linq;
using ConferenceBot.Model.Bing;
using Microsoft.Bot.Connector;

namespace ConferenceBot.Cards
{
    public static class BingSearchCard
    {
        public static IEnumerable<Attachment> GetSearchCards(WebResult webResult)
        {
            return webResult.WebPages.Value.Select(value => new HeroCard
            {
                Title = value.Name,
                Text = value.Snippet,
                Buttons = new List<CardAction>
                {
                    new CardAction
                    {
                        Title = "More",
                        Value = value.Url,
                        Type = ActionTypes.OpenUrl
                    }
                }
            }.ToAttachment());
        }
    }
}