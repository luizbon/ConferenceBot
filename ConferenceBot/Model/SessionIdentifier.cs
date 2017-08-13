using System;

namespace ConferenceBot.Model
{
    [Serializable]
    public class SessionIdentifier
    {
        public DateTime DateTime { get; set; }
        public string Room { get; set; }
    }
}