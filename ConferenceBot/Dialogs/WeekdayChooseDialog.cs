using System;
using System.Linq;
using System.Threading.Tasks;
using ConferenceBot.Extensions;
using ConferenceBot.Model;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Connector;

namespace ConferenceBot.Dialogs
{
    [Serializable]
    public class WeekdayChooseDialog: IDialog<Timeslot[]>
    {
        private readonly Timeslot[] _timeslots;

        public WeekdayChooseDialog(Timeslot[] timeslots)
        {
            _timeslots = timeslots;
        }

        public async Task StartAsync(IDialogContext context)
        {
            var weekdays = _timeslots.Select(t => t.Date.DayOfWeek).Distinct();

            var actions = weekdays.Select(weekday => new CardAction
            {
                Title = weekday.ToString(),
                Type = ActionTypes.ImBack,
                Value = weekday.ToString()
            }).ToList();

            var message = context.CreateMessage();

            message.Text = "I found more than a day for your search, please select which day you looking after";
            message.SuggestedActions = new SuggestedActions
            {
                Actions = actions
            };

            await context.PostAsync(message);

            context.Wait(MessageReceivedAsync);
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (Enum.TryParse(message.Text, out DayOfWeek selectedDate))
            {
                var timeslots = _timeslots.Where(s => s.Date.DayOfWeek == selectedDate).ToArray();
                context.Done(timeslots);
            }
            else
            {
                context.Fail(new Exception());
            }
        }
    }
}