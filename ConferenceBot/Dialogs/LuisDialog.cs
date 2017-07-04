using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using ConferenceBot.Data;
using ConferenceBot.Extensions;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace ConferenceBot.Dialogs
{
    [Serializable]
    public class LuisDialog : LuisDialog<object>
    {
        private const string TitleFilter = "Events.Name";
        private const string TimeFilter = "builtin.datetimeV2.time";
        private const string NextFilter = "next";
        private const string RoomFilter = "room color";
        private const string SpeakerFilter = "speaker";

        public LuisDialog()
            : base(new LuisService(new LuisModelAttribute(ConfigurationManager.AppSettings["ConferenceModelID"],
                ConfigurationManager.AppSettings["LuisSubscriptionID"])))
        {
        }

        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Sorry, I didn't understand");
            await context.PostAsync("I'm still learning how to work better");
            await context.PostAsync("Try asking for a room, speaker or a specific time");
            context.Wait(MessageReceived);
        }

        [LuisIntent("FindTalk")]
        public async Task FindTalk(IDialogContext context, LuisResult result)
        {
            if (!result.Entities.Any())
            {
                await None(context, result);
                return;
            }

            var timeslots = DDDSydney17.Data.Timeslots;

            if (result.TryFindEntity(SpeakerFilter, out EntityRecommendation speaker))
                timeslots = timeslots.FindSpeaker(speaker.Entity);

            if (result.TryFindEntity(TitleFilter, out EntityRecommendation title))
                timeslots = timeslots.FindTitle(title.Entity);

            if (result.TryFindEntity(RoomFilter, out EntityRecommendation room))
                timeslots = timeslots.FindRoom(room.Entity);

            if (result.TryFindTime(TimeFilter, NextFilter, out TimeSpan time))
                timeslots = timeslots.FindTime(time);

            if (!timeslots.Any())
            {
                await context.PostAsync("There's no scheduled talks");
            }
            else
            {
                var message = context.MakeMessage();
                message.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                message.Attachments = Cards.AdaptiveCards.GetSessionCards(timeslots).ToList();
                Console.WriteLine(JsonConvert.SerializeObject(message.Attachments[0]));
                await context.PostAsync(message);
            }
            context.Wait(MessageReceived);
        }
    }
}