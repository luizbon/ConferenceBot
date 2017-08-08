using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConferenceBot.Cards;
using ConferenceBot.Extensions;
using ConferenceBot.Model;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace ConferenceBot.Dialogs
{
    [Serializable]
    public class TimeslotFilterDialog : IDialog<Timeslot[]>
    {
        private Timeslot[] _timeslots;
        public readonly Dictionary<string, IList<SerializableCardAction>> FilterActions = new Dictionary<string, IList<SerializableCardAction>>();

        public TimeslotFilterDialog(Timeslot[] timeslots)
        {
            _timeslots = timeslots;
        }

        public async Task StartAsync(IDialogContext context)
        {
            var sessionCount = _timeslots.SelectMany(t => t.Sessions).Count();
            await context.PostAsync($"I found {sessionCount} sessions, let me see if is possible to narrow this down");
            if (FilterWeekdays() | FilterRooms() | FilterTimes())
            {
                await ActionChooser(context);
                return;
            }

            await context.PostAsync("What a shame, I couldn't help you filter the sessions");
            context.Done(_timeslots);
        }

        private async Task ActionChooser(IDialogContext context)
        {
            if (FilterActions.Count > 1)
            {
                var message = context.CreateMessage();

                message.Text = "How do you want to filter sessions?";
                message.SuggestedActions = new SuggestedActions
                {
                    Actions = FilterActions.Select(a => new CardAction(ActionTypes.ImBack, a.Key, value: a.Key)).ToList()
                };

                await context.PostAsync(message);

                context.Wait(FilterChoose);
                return;
            }

            await ShowFilter(context, FilterActions.First().Value);
        }

        private async Task FilterChoose(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            await ShowFilter(context, FilterActions[message.Text]);
        }

        private async Task ShowFilter(IDialogContext context, IList<SerializableCardAction> filterAction)
        {
            var message = context.CreateMessage();
            message.Text = "Filter sessions by";
            message.SuggestedActions = new SuggestedActions
            {
                Actions = filterAction.Select(x => new CardAction(x.Type, x.Title, value: x.Value)).ToList()
            };

            await context.PostAsync(message);

            context.Wait(FilterSessions);
        }

        private bool FilterWeekdays()
        {
            var weekdays = _timeslots.Select(t => t.Date.DayOfWeek).Distinct().ToList();

            if (weekdays.Count <= 1) return false;

            FilterActions.Add("Day", weekdays.Select(weekday => new SerializableCardAction
            {
                Title = weekday.ToString(),
                Type = ActionTypes.ImBack,
                Value = weekday.ToString()
            }).ToList());

            return true;
        }

        private bool FilterRooms()
        {
            var rooms = _timeslots.SelectMany(t => t.Sessions).Select(s => s.Room.Name).Distinct().ToList();

            if (rooms.Count <= 1) return false;

            FilterActions.Add("Rooms", rooms.Select(room => new SerializableCardAction
            {
                Title = room,
                Type = ActionTypes.ImBack,
                Value = room
            }).ToList());

            return true;
        }

        private bool FilterTimes()
        {
            var times = _timeslots.Select(t => t.Date.TimeOfDay).Distinct().ToList();

            if (times.Count <= 1) return false;

            FilterActions.Add("Time", times.Select(time => new SerializableCardAction
            {
                Title = time.ToString("hh\\:mm"),
                Type = ActionTypes.ImBack,
                Value = time.ToString("hh\\:mm")
            }).ToList());

            return true;
        }

        private async Task FilterSessions(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            if (Enum.TryParse(message.Text, out DayOfWeek selectedDate))
            {
                _timeslots = _timeslots.Where(s => s.Date.DayOfWeek == selectedDate).ToArray();
            }
            else if (TimeSpan.TryParse(message.Text, out TimeSpan selectedTime))
            {
                _timeslots = _timeslots.Where(s => s.Date.TimeOfDay == selectedTime).ToArray();
            }
            else
            {
                foreach (var timeslot in _timeslots)
                    timeslot.Sessions = timeslot.Sessions.Where(s => s.Room.Name == message.Text).ToArray();
            }
            context.Done(_timeslots);
        }
    }
}
