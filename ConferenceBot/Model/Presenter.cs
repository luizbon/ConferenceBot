using System;

namespace ConferenceBot.Model
{
    [Serializable]
    public class Presenter
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string TwitterAlias { get; set; }
        public string[] Bio { get; set; }
        public string Website { get; set; }
        public string ImageUrl { get; set; }
        public string Tag { get; set; }
    }
}