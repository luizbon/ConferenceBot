using System.Collections.Generic;
using System.Linq;
using AdaptiveCards;
using ConferenceBot.Model;
using Microsoft.Bot.Connector;

namespace ConferenceBot.Cards
{
    public static class SessionCard
    {
        public static IEnumerable<Attachment> GetSessionCards(IEnumerable<Timeslot> timeslots)
        {
            return from timeslot in timeslots
                   from session in timeslot.Sessions
                   select CreateAdaptiveCard(session, timeslot)
                into card
                   select new Attachment
                   {
                       ContentType = AdaptiveCard.ContentType,
                       Content = card
                   };
        }

        private static AdaptiveCard CreateAdaptiveCard(Session session, Timeslot timeslot)
        {
            var cardAndAction = AddSessionContainer(session);
            var body = new List<AdaptiveElement>
            {
                AddRoomContainer(session.Room, timeslot),
                cardAndAction.Card
            };

            body.AddRange(session.Presenters.Select((presenter) => PresenterCard.AddPresenterContainer(presenter).Card));

            return new AdaptiveCard
            {
                Speak = session.Title,
                Body = body,
                Actions = new List<AdaptiveAction>
                {
                    cardAndAction.Action
                }
            };
        }


        public static CardAndAction AddSessionContainer(Session session)
        {
            var container = new AdaptiveContainer
            {
                Separator = true,
                Spacing = AdaptiveSpacing.Large,
                Items = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock(session.Title)
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Size = AdaptiveTextSize.Medium,
                        Wrap = true
                    }
                }
            };

            var fullContainer = AddExtraInfoSessionContainer(session);

            var action =
                    new AdaptiveShowCardAction()
                    {
                        Title = "More Info",
                        Card = new AdaptiveCard
                        {
                            Body = new List<AdaptiveElement>
                            {
                                fullContainer
                            }
                        }
                    };

            return new CardAndAction { Card = container, Action = action };
        }

        public static AdaptiveContainer AddExtraInfoSessionContainer(Session session)
        {
            var container = new AdaptiveContainer();

            for (var i = 0; i < session.Abstract.Length; i++)
            {
                var spacing = AdaptiveSpacing.Default;
                if (i > 0 && session.Abstract[i].StartsWith("*") && session.Abstract[i - 1].StartsWith("*"))
                    spacing = AdaptiveSpacing.None;

                container.Items.Add(new AdaptiveTextBlock(session.Abstract[i])
                {
                    Text = session.Abstract[i],
                    Weight = i > 0 && session.Abstract.Length > 1 ? AdaptiveTextWeight.Normal : AdaptiveTextWeight.Bolder,
                    Wrap = true,
                    Separator = false,
                    Spacing = spacing,
                    IsSubtle = i > 0,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Stretch
                });
            }

            return container;
        }

        private static AdaptiveContainer AddRoomContainer(Room room, Timeslot timeslot)
        {
            var container = new AdaptiveContainer
            {
                Items = new List<AdaptiveElement>
                {
                    new AdaptiveColumnSet()
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn()
                            {
                                Width = AdaptiveColumnWidth.Auto.ToLower(),
                                Items =
                                {
                                    new AdaptiveTextBlock(room.Name)
                                    {
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Size = AdaptiveTextSize.ExtraLarge
                                    }
                                }
                            },
                            new AdaptiveColumn()
                            {
                                Width = AdaptiveColumnWidth.Stretch.ToLower(),
                                Items =
                                {
                                    new AdaptiveTextBlock($"{timeslot.Date:ddd HH\\:mm}")
                                    {
                                        HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Size = AdaptiveTextSize.ExtraLarge
                                    }
                                }
                            }
                        }
                    }
                }
            };

            return container;
        }
    }
}