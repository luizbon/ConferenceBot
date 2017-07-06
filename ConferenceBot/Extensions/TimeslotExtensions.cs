using System;
using System.Collections.Generic;
using System.Linq;
using ConferenceBot.Model;

namespace ConferenceBot.Extensions
{
    public static class TimeslotExtensions
    {
        public static Timeslot[] FindSpeaker(this Timeslot[] timeslots, string speaker)
        {
            foreach (var timeslot in timeslots)
                timeslot.Sessions = timeslot.Sessions.Where(s => s.Presenter.Name.IndexOf(speaker,
                                                                     StringComparison
                                                                         .InvariantCultureIgnoreCase) >= 0).ToArray();

            return timeslots;
        }

        public static Timeslot[] FindTitle(this Timeslot[] timeslots, string title)
        {
            foreach (var timeslot in timeslots)
                timeslot.Sessions = timeslot.Sessions.Where(s => s.Title.IndexOf(title,
                                                                     StringComparison
                                                                         .InvariantCultureIgnoreCase) >= 0).ToArray();

            return timeslots;
        }

        public static Timeslot[] FindRoom(this Timeslot[] timeslots, string room)
        {
            foreach (var timeslot in timeslots)
                timeslot.Sessions = timeslot.Sessions.Where(s => string.Equals(s.Room.Name, room,
                    StringComparison
                        .InvariantCultureIgnoreCase)).ToArray();

            return timeslots;
        }

        public static Timeslot[] FindTime(this Timeslot[] timeslots, TimeSpan time)
        {
            var result = new List<Timeslot>();

            var timeslot = timeslots.FirstOrDefault(t => t.Time >= time && t.Sessions.Any());

            if (timeslot != null)
                result.Add(timeslot);

            return result.ToArray();
        }

        public static Timeslot[] FindKeynote(this Timeslot[] timeslots)
        {
            return new[] { timeslots.First() };
        }

        public static Timeslot[] FindLocknote(this Timeslot[] timeslots)
        {
            return new[] { timeslots.Last() };
        }
    }
}