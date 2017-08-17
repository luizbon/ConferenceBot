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
            var body = new List<CardElement>
            {
                AddRoomContainer(session.Room, timeslot),
                AddSessionContainer(session)
            };

            body.AddRange(session.Presenters.Select(PresenterCard.AddPresenterContainer));

            return new AdaptiveCard
            {
                Body = body
            };
        }
        

        public static Container AddSessionContainer(Session session)
        {
            var container = new Container
            {
                Speak = session.Title,
                Separation = SeparationStyle.Strong,
                Items = new List<CardElement>
                {
                    new TextBlock
                    {
                        Text = session.Title,
                        Weight = TextWeight.Bolder,
                        Size = TextSize.Medium,
                        Wrap = true
                    }
                }
            };

            var fullContainer = AddExtraInfoSessionContainer(session);

            var actions = new ActionSet
            {
                Actions = new List<ActionBase>
                {
                    new ShowCardAction
                    {
                        Title = "More Info",
                        Card = new AdaptiveCard
                        {
                            Body = new List<CardElement>
                            {
                                fullContainer
                            }
                        }
                    }
                }
            };

            container.Items.Add(actions);

            return container;
        }

        public static Container AddExtraInfoSessionContainer(Session session)
        {
            var container = new Container();

            for (var i = 0; i < session.Abstract.Length; i++)
            {
                var separation = SeparationStyle.Default;
                if (i > 0 && session.Abstract[i].StartsWith("*") && session.Abstract[i - 1].StartsWith("*"))
                    separation = SeparationStyle.None;

                container.Items.Add(new TextBlock
                {
                    Text = session.Abstract[i],
                    Weight = i > 0 && session.Abstract.Length > 1 ? TextWeight.Normal : TextWeight.Bolder,
                    Wrap = true,
                    Separation = separation,
                    IsSubtle = i > 0,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                });
            }

            return container;
        }

        private static Container AddRoomContainer(Room room, Timeslot timeslot)
        {
            var container = new Container
            {
                Speak = room.Name,
                Items = new List<CardElement>
                {
                    new ColumnSet
                    {
                        Columns = new List<Column>
                        {
                            new Column
                            {
                                Size = "auto",
                                Items =
                                {
                                    new TextBlock
                                    {
                                        Text = room.Name,
                                        Weight = TextWeight.Bolder,
                                        Size = TextSize.ExtraLarge
                                    }
                                }
                            },
                            new Column
                            {
                                Size = "stretch",
                                Items =
                                {
                                    new TextBlock
                                    {
                                        Text = $"{timeslot.Date:ddd HH\\:mm}",
                                        HorizontalAlignment = HorizontalAlignment.Right,
                                        Weight = TextWeight.Bolder,
                                        Size = TextSize.ExtraLarge
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