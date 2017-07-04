using System.Collections.Generic;
using System.Linq;
using AdaptiveCards;
using ConferenceBot.Model;
using Microsoft.Bot.Connector;
using Fact = AdaptiveCards.Fact;

namespace ConferenceBot.Cards
{
    public static class AdaptiveCards
    {
        public static IEnumerable<Attachment> GetSessionCards(IEnumerable<Timeslot> timeslots)
        {
            return from timeslot in timeslots
                from session in timeslot.Sessions
                select new AdaptiveCard
                {
                    Body = new List<CardElement>
                    {
                        AddRoomContainer(session.Room, timeslot),
                        AddSessionContainer(session),
                        AddSpeakerContainer(session.Presenter)
                    },
                    BackgroundImage = session.Room.BackgroundImage
                }
                into card
                select new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card
                };
        }

        private static Container AddSpeakerContainer(Presenter presenter)
        {
            var container = new Container
            {
                Speak = presenter.Name,
                Items = new List<CardElement>
                {
                    new TextBlock
                    {
                        Text = presenter.Name,
                        Weight = TextWeight.Bolder
                    },
                    new TextBlock
                    {
                        Text = presenter.Bio,
                        Wrap = true,
                        Separation = SeparationStyle.None,
                        IsSubtle = true
                    }
                }
            };

            var factSet = new FactSet
            {
                Facts = new List<Fact>()
            };

            if (!string.IsNullOrWhiteSpace(presenter.TwitterAlias))
                factSet.Facts.Add(new Fact
                {
                    Title = "Twitter",
                    Value = presenter.TwitterAlias
                });

            if (!string.IsNullOrWhiteSpace(presenter.Website))
                factSet.Facts.Add(new Fact
                {
                    Title = "Website",
                    Value = presenter.Website
                });

            if (!string.IsNullOrWhiteSpace(presenter.Email))
                factSet.Facts.Add(new Fact
                {
                    Title = "Email",
                    Value = presenter.Email
                });

            container.Items.Add(factSet);

            return container;
        }

        private static Container AddSessionContainer(Session session)
        {
            return new Container
            {
                Speak = session.Title,
                Items = new List<CardElement>
                {
                    new TextBlock
                    {
                        Text = session.Title,
                        Weight = TextWeight.Bolder,
                        Size = TextSize.Medium,
                        Wrap = true
                    },
                    new TextBlock
                    {
                        Text = session.Abstract,
                        Wrap = true,
                        Separation = SeparationStyle.None,
                        IsSubtle = true
                    }
                }
            };
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
                                        Text = $"{timeslot.Time:hh\\:mm}",
                                        HorizontalAlignment = HorizontalAlignment.Right,
                                        Weight = TextWeight.Bolder,
                                        Size = TextSize.ExtraLarge
                                    }
                                }
                            }
                        }
                    },
                    new TextBlock
                    {
                        Text = room.Reference,
                        Wrap = true,
                        Separation = SeparationStyle.None,
                        IsSubtle = true
                    }
                }
            };

            return container;
        }
    }
}