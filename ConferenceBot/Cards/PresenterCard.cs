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
            var cardAndAction = AddPresenterContainer(presenter);

            var body = new List<AdaptiveElement>
            {
                cardAndAction.Card
            };

            if (timeslots != null)
            {
                body.AddRange(timeslots.SelectMany(t => t.Sessions).Where(s => s.Presenters.Any(p => p.Name == presenter.Name)).Select((session) => SessionCard.AddSessionContainer(session).Card));
            }

            return new AdaptiveCard
            {
                Speak = presenter.Name,
                Body = body,
                Actions = new List<AdaptiveAction>
                {
                    cardAndAction.Action
                }
            };
        }

        public static CardAndAction AddPresenterContainer(Presenter presenter)
        {
            var container = new AdaptiveContainer
            {
                Separator = true,
                Spacing = AdaptiveSpacing.Large,
                Items = new List<AdaptiveElement>
                {
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage(presenter.ImageUrl)
                                    {
                                        Style = AdaptiveImageStyle.Person,
                                        Size = AdaptiveImageSize.Medium
                                    }
                                }
                            },
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Stretch,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock(presenter.Name)
                                    {
                                        Weight = AdaptiveTextWeight.Bolder,
                                        Size = AdaptiveTextSize.Medium
                                    }
                                }
                            }
                        }
                    }
                }
            };

            container.Items.Add(
                new AdaptiveTextBlock(presenter.Tag)
                {
                    IsSubtle = true,
                    Separator = false,
                    Spacing = AdaptiveSpacing.None,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right
                });

            var actions =
                        new AdaptiveShowCardAction()
                        {
                            Title = "More Info",
                            Card = CreateExtraInfoCard(presenter)
                        };

            return new CardAndAction { Card = container, Action = actions };
        }

        private static AdaptiveCard CreateExtraInfoCard(Presenter presenter)
        {
            var container = new AdaptiveContainer();
            foreach (var bioLine in presenter.Bio)
                container.Items.Add(new AdaptiveTextBlock(bioLine)
                {
                    Wrap = true,
                    Separator = false,
                    Spacing = AdaptiveSpacing.Default,
                    IsSubtle = true,
                    HorizontalAlignment = AdaptiveHorizontalAlignment.Stretch
                });

            var twitterUrl = "https://twitter.com/intent/tweet?hashtags=ndcsydney&via=luizbon";
            if (!string.IsNullOrWhiteSpace(presenter.TwitterAlias))
                twitterUrl += $"&screen_name={presenter.TwitterAlias}";

            var actions = new List<AdaptiveAction>
                    {
                        new AdaptiveOpenUrlAction
                        {
                            Title = "Tweet",
                            UrlString = twitterUrl
                        }
                    };

            if (!string.IsNullOrWhiteSpace(presenter.Website))
                actions.Add(new AdaptiveOpenUrlAction
                {
                    Title = "Website",
                    UrlString = presenter.Website
                });

            if (!string.IsNullOrWhiteSpace(presenter.Email))
                actions.Add(new AdaptiveOpenUrlAction
                {
                    Title = "Email",
                    UrlString = $"mailto:{presenter.Email}"
                });

            return new AdaptiveCard
            {
                Body = new List<AdaptiveElement>
                {
                    container
                },
                Actions = actions
            };
        }
    }
}