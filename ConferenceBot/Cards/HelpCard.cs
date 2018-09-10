using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AdaptiveCards;

namespace ConferenceBot.Cards
{
    public class HelpCard : AdaptiveCard
    {
        public HelpCard(string speakerName, string room)
        {
            Body = new List<AdaptiveElement>
            {
                new AdaptiveTextBlock("Please ask me about talks, rooms and speakers.")
                {
                    Size = AdaptiveTextSize.Medium
                },
                new AdaptiveTextBlock("Here are some examples of what you can ask.")
                {
                    Size = AdaptiveTextSize.Medium
                }
            };

            Actions = new List<AdaptiveAction>
            {
                new AdaptiveSubmitAction
                {
                    Title = "Speaker Example",
                    Data = $"When is {speakerName}'s talk?"
                },
                new AdaptiveSubmitAction
                {
                    Title = "Room Example",
                    Data = $"What's happening on {room} today?"
                },
                new AdaptiveSubmitAction
                {
                    Title = "Time Example",
                    Data = "What's going on at 3PM today?"
                }
            };
        }
    }
}