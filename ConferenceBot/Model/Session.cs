namespace ConferenceBot.Model
{
    public class Session
    {
        public string Title { get; set; }
        public string Abstract { get; set; }
        public Presenter Presenter { get; set; }
        public Room Room { get; set; }
    }
}