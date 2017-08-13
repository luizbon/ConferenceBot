using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using ConferenceBot.Model;
using Newtonsoft.Json;

namespace ConferenceBot.Data
{
    public class NdcSydney17
    {
        public static Conference Data => ToConference(JsonConvert.DeserializeObject<NdcSydney17>(File.ReadAllText(
            HttpContext.Current.Request.MapPath("~\\Data\\NDCSydney17.json"))));

        public static string Lat = "-33.8718413";
        public static string Long = "151.2078864";

        public static string[] Speakers = Data.Timeslots.SelectMany(t => t.Sessions).SelectMany(s => s.Presenters)
            .Select(p => p.Name).Distinct().OrderBy(speaker => speaker.Replace(" ", "")).ToArray();

        public static string[] Rooms = Data.Timeslots.SelectMany(t => t.Sessions)
            .Where(s => !string.IsNullOrWhiteSpace(s.Room.Name)).Select(s => s.Room.Name)
            .Distinct().OrderBy(room => room).ToArray();

        private static Conference ToConference(NdcSydney17 dddSydney)
        {
            return new Conference
            {
                Timeslots = dddSydney.Timeslots.Select(ToTimeslot).ToArray()
            };
        }

        private static Model.Timeslot ToTimeslot(Timeslot timeslot)
        {
            return new Model.Timeslot
            {
                Break = timeslot.Break,
                Date = timeslot.Date.Add(timeslot.Time),
                Title = timeslot.Title,
                Sessions = timeslot.Sessions.Select(ToSession).ToArray(),
                IsKeynote = timeslot.IsKeynote,
                IsLocknote = timeslot.IsLocknote
            };
        }

        private static Model.Session ToSession(Session session)
        {
            return new Model.Session
            {
                Title = session.Tittle,
                Abstract = session.Abstract,
                Presenters = session.Presenters.Select(ToPresenter).ToArray(),
                Room = new Room
                {
                    Name = session.Room ?? " "
                },
                
            };
        }

        private static Model.Presenter ToPresenter(Presenter presenter)
        {
            return new Model.Presenter
            {
                Name = presenter.Name,
                Bio = presenter.Bio,
                TwitterAlias = presenter.TwitterAlias,
                Website = presenter.Website,
                ImageUrl = presenter.ImageUrl,
                Tag = presenter.Tag
            };
        }

        [JsonProperty("timeslots")]
        public Timeslot[] Timeslots { get; set; }

        public class Timeslot
        {
            [JsonProperty("time")]
            private string JsonTime { get; set; }

            [JsonIgnore]
            public TimeSpan Time => TimeSpan.Parse(JsonTime, new CultureInfo("en-au"));

            [JsonProperty]
            public DateTime Date { get; set; }

            [JsonProperty("sessions")]
            public Session[] Sessions { get; set; } = { };

            [JsonProperty("break")]
            public bool Break { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("isKeynote")]
            public bool IsKeynote { get; set; }

            [JsonProperty("isLocknote")]
            public bool IsLocknote { get; set; }
        }

        public class Session
        {
            [JsonProperty("title")]
            public string Tittle { get; set; }
            [JsonProperty("abstract")]
            public string[] Abstract { get; set; }
            [JsonProperty("level")]
            public string Level { get; set; }
            [JsonProperty("tags")]
            public string[] Tags { get; set; }
            [JsonProperty("room")]
            public string Room { get; set; }
            [JsonProperty("presenters")]
            public Presenter[] Presenters { get; set; }
        }

        public class Presenter
        {
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("bio")]
            public string[] Bio { get; set; }
            [JsonProperty("twitterAlias")]
            public string TwitterAlias { get; set; }
            [JsonProperty("website")]
            public string Website { get; set; }
            [JsonProperty("imageUrl")]
            public string ImageUrl { get; set; }
            [JsonProperty("tag")]
            public string Tag { get; set; }
        }
    }
}