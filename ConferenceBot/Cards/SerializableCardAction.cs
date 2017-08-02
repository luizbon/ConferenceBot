using System;

namespace ConferenceBot.Cards
{
    [Serializable]
    public class SerializableCardAction
    {
        public SerializableCardAction(string type, string title)
        {
            Type = type;
            Title = title;
            Value = title;
        }

        public SerializableCardAction()
        {
        }

        public string Type { get; set; }
        public string Title { get; set; }
        public object Value { get; set; }

    }
}