using System;
using System.Collections.Generic;
using System.Linq;
using Chronic;
using ConferenceBot.Model;

namespace ConferenceBot.Extensions
{
    public static class TimeslotExtensions
    {
        public static Timeslot[] FindSpeaker(this Timeslot[] timeslots, string speaker)
        {
            foreach (var timeslot in timeslots)
                timeslot.Sessions = timeslot.Sessions.Where(s => s.Presenters.Any(p => p.Name.IndexOf(speaker,
                                                                     StringComparison
                                                                         .InvariantCultureIgnoreCase) >= 0)).ToArray();

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

            var days = timeslots.Select(t => t.Date.Date).Distinct();

            foreach (var day in days)
            {
                var timeslot = timeslots.Where(t => t.Date.Date == day).FirstOrDefault(t => t.Date.TimeOfDay >= time && t.Sessions.Any());

                if (timeslot != null)
                    result.Add(timeslot);
            }

            return result.ToArray();
        }

        public static Timeslot[] FindDate(this Timeslot[] timeslots, DateTime startDate, DateTime endDate)
        {
            return timeslots.Where(t => t.Date >= startDate && t.Date <= endDate && t.Sessions.Any()).ToArray();
        }

        public static Timeslot[] FindKeynote(this Timeslot[] timeslots)
        {
            return timeslots.Where(t => t.IsKeynote).ToArray();
        }

        public static Timeslot[] FindLocknote(this Timeslot[] timeslots)
        {
            return timeslots.Where(t => t.IsLocknote).ToArray();
        }

        public static SessionIdentifier[] ToSessionIdentifiers(this Timeslot[] timeslots)
        {
            return (from timeslot in timeslots
                    from session in timeslot.Sessions
                    select new SessionIdentifier
                    {
                        DateTime = timeslot.Date,
                        Room = session.Room.Name
                    }).ToArray();
        }

        public static Timeslot[] FromSessionIdentifiers(this Timeslot[] timeslots,
            SessionIdentifier[] sessionIdentifiers)
        {
            return (from timeslot in timeslots
                    from sessionIdentifier in sessionIdentifiers
                    where timeslot.Date == sessionIdentifier.DateTime
                    let sessions = timeslot.Sessions.Where(s => s.Room.Name == sessionIdentifier.Room)
                    select new Timeslot
                    {
                        Date = timeslot.Date,
                        IsKeynote = timeslot.IsKeynote,
                        Title = timeslot.Title,
                        Sessions = sessions.ToArray(),
                        Break = timeslot.Break,
                        IsLocknote = timeslot.IsLocknote
                    }).ToArray();
        }
    }
}