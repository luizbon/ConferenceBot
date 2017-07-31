using System;

namespace ConferenceBot.Model
{
    [Serializable]
    public class Session
    {
        public string Title { get; set; }
        public string Abstract { get; set; }
        public Presenter[] Presenters { get; set; }
        public Room Room { get; set; }
    }
}