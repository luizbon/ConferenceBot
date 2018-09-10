﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using AdaptiveCards;
using ConferenceBot.Cards;
using ConferenceBot.Data;
using ConferenceBot.Extensions;
using ConferenceBot.Model;
using ConferenceBot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;

namespace ConferenceBot.Dialogs
{
    [Serializable]
    public class LuisDialog : LuisDialog<object>
    {
        private const string TitleFilter = "Events.Name";
        private const string TimeFilter = "builtin.datetimeV2.time";
        private const string DateTimeFilter = "builtin.datetimeV2.datetime";
        private const string TimeRangeFilter = "builtin.datetimeV2.datetimerange";
        private const string DateFilter = "builtin.datetimeV2.date";
        private const string NextFilter = "next";
        private const string RoomFilter = "room";
        private const string SpeakerFilter = "speaker";
        private const string KeynoteFilter = "keynote";
        private const string LocknoteFilter = "locknote";

        public LuisDialog(bool staging)
            : base(new LuisService(new LuisModelAttribute(ConfigurationManager.AppSettings["ConferenceModelID"],
                ConfigurationManager.AppSettings["LuisSubscriptionID"])
            {
                Staging = staging
            }))
        {
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Sorry, I didn't understand");
            await ShowHelp(context);
            context.Wait(MessageReceived);
        }

        [LuisIntent("FindSpeaker")]
        public async Task FindSpeaker(IDialogContext context, LuisResult result)
        {
            if (await NoEntities(context, result)) return;

            var timeslots = FilterTimeslots(result, out bool isNext);

            var presenters = timeslots.GetPresenters();

            var totalPresenters = presenters.Length;

            if (totalPresenters <= 0 && isNext)
            {
                await context.PostAsync(
                    "Sorry, but I'm afraid there are no more sessions for today. Please check again later.");
                context.Wait(MessageReceived);
            }
            else if (totalPresenters <= 0)
            {
                await context.PostAsync("Hang on a sec while I check for you");
                await SearchWeb(context, result.Query);
                context.Wait(MessageReceived);
            }
            else
            {
                await context.PostAsync("Let's see what I have here.");
                await context.SendTyping();

                if (totalPresenters > 7)
                {
                    context.Call(new TimeslotFilterDialog(timeslots.ToSessionIdentifiers()), PresentersResumeAsync);
                    return;
                }

                await SendPresenters(context, timeslots);
            }
        }

        [LuisIntent("FindTalk")]
        public async Task FindTalk(IDialogContext context, LuisResult result)
        {
            if (await NoEntities(context, result)) return;

            var timeslots = FilterTimeslots(result, out bool isNext);

            var totalSessions = timeslots.SelectMany(t => t.Sessions).Count();

            if (totalSessions <= 0 && !isNext)
            {
                await context.PostAsync("Hang on a sec while I check for you");
                await SearchWeb(context, result.Query);
                context.Wait(MessageReceived);
            }
            else if (totalSessions <= 0 && isNext)
            {
                await context.PostAsync(
                    "Sorry, but I'm afraid there are no more sessions for today. Please check again later.");
                context.Wait(MessageReceived);
            }
            else
            {
                await context.PostAsync("So you are looking for a talk?\n\nLet's see what I have here.");
                await context.SendTyping();

                if (totalSessions > 7)
                {
                    context.Call(new TimeslotFilterDialog(timeslots.ToSessionIdentifiers()), TalksResumeAsync);
                    return;
                }

                await SendTalks(context, timeslots);
            }
        }

        private static async Task<bool> NoEntities(IDialogContext context, LuisResult result)
        {
            if (result.Entities.Any()) return false;

            if (await SearchQnA(context, result.Query)) return true;

            await context.PostAsync("You need to be a bit more specific");
            await ShowHelp(context);

            return true;
        }

        private static Timeslot[] FilterTimeslots(LuisResult result, out bool isNext)
        {
            var timeslots = NdcSydney.Data.Timeslots;

            if (result.TryFindEntity(KeynoteFilter, out EntityRecommendation _))
                timeslots = timeslots.FindKeynote();

            if (result.TryFindEntity(LocknoteFilter, out EntityRecommendation _))
                timeslots = timeslots.FindLocknote();

            if (result.TryFindEntity(SpeakerFilter, out EntityRecommendation speaker))
                timeslots = timeslots.FindSpeaker(speaker.Entity);

            if (result.TryFindEntity(TitleFilter, out EntityRecommendation title))
                timeslots = timeslots.FindTitle(title.Entity);

            if (result.TryFindEntity(RoomFilter, out EntityRecommendation room))
                timeslots = timeslots.FindRoom(room.Entity);

            if (result.TryFindTime(TimeFilter, NextFilter, out TimeSpan time, out isNext))
                timeslots = timeslots.FindTime(time, isNext);

            if (result.TryFindDate(TimeRangeFilter, out DateTime startDateTimeRange, out DateTime endDateTimeRange))
                timeslots = timeslots.FindDate(startDateTimeRange, endDateTimeRange);

            if (result.TryFindDate(DateFilter, out DateTime startDate, out DateTime endDate))
                timeslots = timeslots.FindDate(startDate, endDate);

            if (result.TryFindDateTime(DateTimeFilter, out DateTime dateTime))
            {
                timeslots = timeslots.FindTime(dateTime.TimeOfDay, false);
                timeslots = timeslots.FindDate(dateTime.Date, dateTime.Date.AddDays(1));
            }

            return timeslots;
        }

        private async Task SendPresenters(IDialogContext context, Timeslot[] timeslots)
        {
            var presenters = timeslots.GetPresenters();
            if (presenters.Length > 0)
                await context.PostAsync("Good news, I found some speakers");
            else
                await context.PostAsync("Good news, I found the speaker you looking after");

            var message = context.CreateMessage(PresenterCard.GetPresenterCards(timeslots).ToList());
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        private async Task PresentersResumeAsync(IDialogContext context, IAwaitable<SessionIdentifier[]> result)
        {
            try
            {
                var sessionIdentifiers = await result;

                await SendPresenters(context, NdcSydney.Data.Timeslots.FromSessionIdentifiers(sessionIdentifiers));
            }
            catch (Exception)
            {
                await context.PostAsync("I'm sorry, I'm having issues understanding you. Let's try again.");

                await ShowHelp(context);
            }
        }

        private async Task SendTalks(IDialogContext context, Timeslot[] timeslots)
        {
            if (timeslots.SelectMany(x => x.Sessions).Count() > 1)
                await context.PostAsync("Good news, I found some talks");
            else
                await context.PostAsync("Good news, I found one talk");
            var message = context.CreateMessage(SessionCard.GetSessionCards(timeslots).ToList());
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        private async Task TalksResumeAsync(IDialogContext context, IAwaitable<SessionIdentifier[]> result)
        {
            try
            {
                var sessionIdentifiers = await result;

                await SendTalks(context, NdcSydney.Data.Timeslots.FromSessionIdentifiers(sessionIdentifiers));
            }
            catch (Exception)
            {
                await context.PostAsync("I'm sorry, I'm having issues understanding you. Let's try again.");

                await ShowHelp(context);
            }
        }

        [LuisIntent("ListRooms")]
        public async Task ListRooms(IDialogContext context, LuisResult result)
        {
            var actions = NdcSydney.Rooms().Select(room => new CardAction
            {
                Title = room,
                Type = ActionTypes.ImBack,
                Value = $"What is schedule for {room} room?"
            }).ToList();

            var message = context.CreateMessage();

            message.Text = "Here is a list of available rooms:";
            message.SuggestedActions = new SuggestedActions
            {
                Actions = actions
            };

            await context.PostAsync(message);

            context.Wait(MessageReceived);
        }

        [LuisIntent("ListSpeakers")]
        public async Task ListSpeakers(IDialogContext context, LuisResult result)
        {
            var initials = NdcSydney.Speakers().GroupBy(s => s.ToLower()[0]);

            await context.PostAsync($"There are {NdcSydney.Speakers().Length} speakers, I'll group them by initials");

            foreach (var initial in initials)
            {
                await context.SendTyping();
                await context.PostAsync(string.Join("\n\n", initial));
            }

            var speakerIndex = new Random().Next(0, NdcSydney.Speakers().Length);
            await context.PostAsync($"Try asking: When is {NdcSydney.Speakers()[speakerIndex]}'s talk?");

            context.Wait(MessageReceived);
        }

        [LuisIntent("BingSearch")]
        public async Task BingSearch(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hang on a sec while I check for you");
            await SearchWeb(context, result.Query);
            context.Wait(MessageReceived);
        }

        private static async Task SearchWeb(IDialogContext context, string query)
        {
            if (await SearchQnA(context, query))
                return;

            await context.SendTyping();
            var search = new BindSearchService();
            var searchResult = await search.Search(query);
            var attachments = BingSearchCard.GetSearchCards(searchResult);

            await context.PostAsync("Here it goes, this is what I found.");
            await context.PostAsync(context.CreateMessage(attachments.ToList()));
        }

        private static async Task<bool> SearchQnA(IDialogContext context, string query)
        {
            await context.SendTyping();
            var qnA = new QnAService();
            var searchResult = await qnA.Search(query);

            if (searchResult == null || Math.Abs(searchResult.Score) < double.Epsilon) return false;

            await context.PostAsync(searchResult.Answer.Replace("\\n", "\n"));
            return true;
        }

        [LuisIntent("FindVenue")]
        public async Task FindVenue(IDialogContext context, LuisResult result)
        {
            var location = $"{NdcSydney.Lat},{NdcSydney.Long}";
            var googleApiKey = ConfigurationManager.AppSettings["GoogleApiKey"];
            var mapUrl =
                $"https://maps.googleapis.com/maps/api/staticmap?center={location}&zoom=17&size=600x300&maptype=roadmap&markers=color:red%7Clabel:DDD%7C{location}&key={googleApiKey}";

            var card = new HeroCard("NDC Sydney", "Hilton Sydney",
                "NDC Sydney 2018 is set to happen 17-21 September at Hilton Sydney.")
            {
                Images = new List<CardImage>
                {
                    new CardImage(mapUrl)
                },
                Buttons = new List<CardAction>
                {
                    new CardAction
                    {
                        Title = "Get Directions",
                        Type = ActionTypes.OpenUrl,
                        Value =
                            $"https://www.google.com.au/maps/dir//{location}/@{location},19z/data=!4m8!1m7!3m6!1s0x0:0x0!2zMzPCsDUyJzU5LjYiUyAxNTHCsDEyJzA2LjUiRQ!3b1!8m2!3d{NdcSydney.Lat}!4d{NdcSydney.Long}"
                    }
                }
            };

            await context.PostAsync(context.CreateMessage(card.ToAttachment()));

            context.Wait(MessageReceived);
        }

        [LuisIntent("Greetings")]
        public async Task Greetings(IDialogContext context, LuisResult result)
        {
            await context.PostAsync(Messages.Greetings());

            await ShowHelp(context);

            context.Wait(MessageReceived);
        }

        [LuisIntent("Farewell")]
        public async Task Farewell(IDialogContext context, LuisResult result)
        {
            await context.PostAsync(Messages.Farewell());

            context.Wait(MessageReceived);
        }

        [LuisIntent("FindWorkshop")]
        public async Task FindWorkshop(IDialogContext context, LuisResult result)
        {
            await context.PostAsync(
                "Humm, I don't have any information about workshops yet.\n\nMy lazy developer didn't input the data.\n\nPlease visit the official web-page to get more info on http://ndcsydney.com/workshops/");

            context.Wait(MessageReceived);
        }

        private static async Task ShowHelp(IDialogContext context)
        {
            var speakerIndex = new Random().Next(0, NdcSydney.Speakers().Length);
            var roomIndex = new Random().Next(0, NdcSydney.Rooms().Length);


            var message = context.CreateMessage();

            var helpCard = new HelpCard(NdcSydney.Speakers()[speakerIndex], NdcSydney.Rooms()[roomIndex]);

            message.Attachments = new List<Attachment>
            {
                new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = helpCard
                }
            };

            await context.PostAsync(message);
        }
    }
}