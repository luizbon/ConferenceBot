using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using ConferenceBot.Cards;
using ConferenceBot.Data;
using ConferenceBot.Extensions;
using ConferenceBot.Model;
using ConferenceBot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using CardAction = Microsoft.Bot.Connector.CardAction;

namespace ConferenceBot.Dialogs
{
    [Serializable]
    public class LuisDialog : LuisDialog<object>
    {
        private const string TitleFilter = "Events.Name";
        private const string TimeFilter = "builtin.datetimeV2.time";
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

        [LuisIntent("FindTalk")]
        public async Task FindTalk(IDialogContext context, LuisResult result)
        {
            if (!result.Entities.Any())
            {
                if (await SearchQnA(context, result.Query)) return;

                await context.PostAsync("You need to be a bit more specific");
                await ShowHelp(context);
                return;
            }

            await context.PostAsync("So you are looking for a talk?\n\nLet's see what I have here.");
            await context.SendTyping();

            var timeslots = NdcSydney17.Data.Timeslots;

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

            if (result.TryFindTime(TimeFilter, NextFilter, out TimeSpan time))
                timeslots = timeslots.FindTime(time);

            if (result.TryFindDate(DateFilter, out DateTime startDate, out DateTime endDate))
            {
                timeslots = timeslots.FindDate(startDate, endDate);
            }

            var totalSessions = timeslots.SelectMany(t => t.Sessions).Count();

            if (totalSessions <= 0)
            {
                await context.PostAsync("Sorry, but I could not find what you are looking for"); ;
                await SearchWeb(context, result.Query);
                context.Wait(MessageReceived);
            }
            else
            {
                if (totalSessions > 7)
                {
                    context.Call(new TimeslotFilterDialog(timeslots), FilterResumeAsync);
                    return;
                }

                await SendTalks(context, timeslots);
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

        private async Task FilterResumeAsync(IDialogContext context, IAwaitable<Timeslot[]> result)
        {
            try
            {
                var timeslots = await result;

                await SendTalks(context, timeslots);
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
            var actions = NdcSydney17.Rooms.Select(room => new CardAction
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
            var actions = NdcSydney17.Speakers.Select(speaker => new CardAction
            {
                Title = speaker,
                Type = ActionTypes.ImBack,
                Value = $"When is {speaker}'s talk?"
            }).ToList();

            var message = context.CreateMessage();

            message.Text = "This is the list of speakers we have this year:";
            message.SuggestedActions = new SuggestedActions
            {
                Actions = actions
            };

            await context.PostAsync(message);

            context.Wait(MessageReceived);
        }

        [LuisIntent("BingSearch")]
        public async Task BingSearch(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Wait a sec while I search more about it.");
            await SearchWeb(context, result.Query);
            context.Wait(MessageReceived);
        }

        private static async Task SearchWeb(IDialogContext context, string query)
        {
            if (await SearchQnA(context, query))
                return;

            await context.SendTyping();
            var search = new BindSearchService();
            var searchResult = await search.Search($"NDC Sydney 2017: {query}");
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
            var location = $"{NdcSydney17.Lat},{NdcSydney17.Long}";
            var googleApiKey = ConfigurationManager.AppSettings["GoogleApiKey"];
            var mapUrl =
                $"https://maps.googleapis.com/maps/api/staticmap?center={location}&zoom=17&size=600x300&maptype=roadmap&markers=color:red%7Clabel:DDD%7C{location}&key={googleApiKey}";

            var card = new HeroCard("NDC Sydney", "Holton Sydney",
                "NDC Sydney 2017 is set to happen 14-18 August at Hilton Sydney.")
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
                            $"https://www.google.com.au/maps/dir//{location}/@{location},19z/data=!4m8!1m7!3m6!1s0x0:0x0!2zMzPCsDUyJzU5LjYiUyAxNTHCsDEyJzA2LjUiRQ!3b1!8m2!3d{NdcSydney17.Lat}!4d{NdcSydney17.Long}"
                    }
                }
            };

            await context.PostAsync(context.CreateMessage(card.ToAttachment()));

            context.Wait(MessageReceived);
        }

        [LuisIntent("Greetings")]
        public async Task Greetings(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Hi there!\n\n" +
                                    "I'm here to help you\n\n");
            await ShowHelp(context);
            context.Wait(MessageReceived);
        }

        private static async Task ShowHelp(IBotToUser context)
        {
            var speakerIndex = new Random().Next(0, NdcSydney17.Speakers.Length);
            var roomIndex = new Random().Next(0, NdcSydney17.Rooms.Length);
            await context.PostAsync("You can ask me about talks, rooms and speakers.\n\n" +
                                    $"Try asking: When is {NdcSydney17.Speakers[speakerIndex]}'s talk?\n\n" +
                                    "or\n\n" +
                                    $"What's happening on {NdcSydney17.Rooms[roomIndex]}?\n\n" +
                                    "or\n\n" +
                                    "What's going on at 3PM?");
        }
    }
}