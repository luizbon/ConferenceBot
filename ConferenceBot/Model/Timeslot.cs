using System;

namespace ConferenceBot.Model
{
    public class Timeslot
    {
        public TimeSpan Time { get; set; }

        public bool Break { get; set; }

        public string Title { get; set; }

        public Session[] Sessions { get; set; }
    }
}