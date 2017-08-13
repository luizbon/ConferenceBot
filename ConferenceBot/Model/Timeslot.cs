using System;

namespace ConferenceBot.Model
{
    [Serializable]
    public class Timeslot
    {
        public DateTime Date { get; set; }

        public bool Break { get; set; }

        public string Title { get; set; }

        public Session[] Sessions { get; set; }

        public bool IsKeynote { get; set; }

        public bool IsLocknote { get; set; }
    }
}