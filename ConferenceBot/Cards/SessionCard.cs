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

            body.AddRange(session.Presenters.Select(AddSpeakerContainer));

            return new AdaptiveCard
            {
                Body = body
            };
        }

        private static Container AddSpeakerContainer(Presenter presenter)
        {
            var container = new Container
            {
                Speak = presenter.Name,
                Separation = SeparationStyle.Strong,
                Items = new List<CardElement>
                {
                    new ColumnSet
                    {
                        Columns = new List<Column>
                        {
                            new Column
                            {
                                Size = ColumnSize.Auto,
                                Items = new List<CardElement>
                                {
                                    new Image
                                    {
                                        Url = presenter.ImageUrl,
                                        Style = ImageStyle.Person
                                    }
                                }
                            },
                            new Column
                            {
                                Size = ColumnSize.Stretch,
                                Items = new List<CardElement>
                                {
                                    new TextBlock
                                    {
                                        Text = presenter.Name,
                                        Weight = TextWeight.Bolder,
                                        Size = TextSize.Medium
                                    }
                                }
                            }
                        }
                    }
                }
            };

            container.Items.Add(
                new TextBlock
                {
                    Text = presenter.Tag,
                    IsSubtle = true,
                    Separation = SeparationStyle.None,
                    HorizontalAlignment = HorizontalAlignment.Right
                });

            foreach (var bioLine in presenter.Bio)
                container.Items.Add(new TextBlock
                {
                    Text = bioLine,
                    Wrap = true,
                    Separation = SeparationStyle.Default,
                    IsSubtle = true,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                });

            var twitterUrl = "https://twitter.com/intent/tweet?hashtags=ndcsydney";
            if (!string.IsNullOrWhiteSpace(presenter.TwitterAlias))
                twitterUrl += $",{presenter.TwitterAlias}";

            var actions = new ActionSet
            {
                Actions = new List<ActionBase>
                {
                    new OpenUrlAction
                    {
                        Title = "Tweet",
                        Url = twitterUrl
                    }
                }
            };

            if (!string.IsNullOrWhiteSpace(presenter.Website))
                actions.Actions.Add(new OpenUrlAction
                {
                    Title = "Website",
                    Url = presenter.Website
                });

            if (!string.IsNullOrWhiteSpace(presenter.Email))
                actions.Actions.Add(new OpenUrlAction
                {
                    Title = "Email",
                    Url = $"mailto:{presenter.Email}"
                });

            container.Items.Add(actions);

            return container;
        }

        private static Container AddSessionContainer(Session session)
        {
            var container = new Container
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
                    }
                }
            };

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