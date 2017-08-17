using System.Collections.Generic;
using System.Linq;
using AdaptiveCards;
using ConferenceBot.Extensions;
using ConferenceBot.Model;
using Microsoft.Bot.Connector;

namespace ConferenceBot.Cards
{
    public static class PresenterCard
    {
        public static IEnumerable<Attachment> GetPresenterCards(Timeslot[] timeslots)
        {
            var presenters = timeslots.GetPresenters();

            return from presenter in presenters
                   select CreatePresenterCard(presenter, timeslots)
                   into card
                   select new Attachment
                   {
                       ContentType = AdaptiveCard.ContentType,
                       Content = card
                   };
        }

        private static AdaptiveCard CreatePresenterCard(Presenter presenter, Timeslot[] timeslots = null)
        {
            var body = new List<CardElement>
            {
                AddPresenterContainer(presenter)
            };

            if (timeslots != null)
            {
                body.AddRange(timeslots.SelectMany(t => t.Sessions).Where(s => s.Presenters.Any(p => p.Name == presenter.Name)).Select(SessionCard.AddSessionContainer));
            }

            return new AdaptiveCard
            {
                Body = body
            };
        }

        public static Container AddPresenterContainer(Presenter presenter)
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

            var actions = new ActionSet
            {
                Actions = new List<ActionBase>
                    {
                        new ShowCardAction
                        {
                            Title = "More Info",
                            Card = CreateExtraInfoCard(presenter)
                        }
                    }
            };

            container.Items.Add(actions);

            return container;
        }

        private static AdaptiveCard CreateExtraInfoCard(Presenter presenter)
        {
            var container = new Container();
            foreach (var bioLine in presenter.Bio)
                container.Items.Add(new TextBlock
                {
                    Text = bioLine,
                    Wrap = true,
                    Separation = SeparationStyle.Default,
                    IsSubtle = true,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                });

            var twitterUrl = "https://twitter.com/intent/tweet?hashtags=ndcsydney&via=luizbon";
            if (!string.IsNullOrWhiteSpace(presenter.TwitterAlias))
                twitterUrl += $"&screen_name={presenter.TwitterAlias}";

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

            return new AdaptiveCard
            {
                Body = new List<CardElement>
                {
                    container
                }
            };
        }
    }
}