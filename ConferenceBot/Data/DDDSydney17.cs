using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using ConferenceBot.Model;
using Newtonsoft.Json;

namespace ConferenceBot.Data
{
    public class DDDSydney17
    {
        public static Conference Data => ToConference(JsonConvert.DeserializeObject<DDDSydney17>(File.ReadAllText(
            HttpContext.Current.Request.MapPath("~\\Data\\dddSydney17.json"))));

        private static Conference ToConference(DDDSydney17 dddSydney)
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
                Time = timeslot.Time,
                Title = timeslot.Title,
                Sessions = timeslot.Sessions.Select(ToSession).ToArray()
            };
        }

        private static Model.Session ToSession(Session session)
        {
            return new Model.Session
            {
                Title = session.SessionTitle,
                Abstract = session.SessionAbstract,
                Presenter = new Presenter
                {
                    Name = session.PresenterName,
                    Bio = session.PresenterBio,
                    Email = session.PresenterEmail,
                    TwitterAlias = session.PresenterTwitterAlias,
                    Website = session.PresenterWebsite
                },
                Room = new Room
                {
                    Name = session.RoomColor ?? " ",
                    Reference = session.Room ?? " ",
                    BackgroundImage = session.RoomBackground
                }
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

            [JsonProperty("sessions")]
            public Session[] Sessions { get; set; } = { };

            [JsonProperty("break")]
            public bool Break { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }
        }

        public class Session
        {
            public string SessionTitle { get; set; }
            public string PresenterName { get; set; }
            public string SessionAbstract { get; set; }
            public string PresenterEmail { get; set; }
            public string PresenterTwitterAlias { get; set; }
            public string RecommendedAudience { get; set; }
            public string PresenterBio { get; set; }
            public string PresenterWebsite { get; set; }
            public string RoomColor { get; set; }
            public string Room { get; set; }

            public string RoomBackground
            {
                get
                {
                    switch (RoomColor)
                    {
                        case "Green":
                            return "http://www.colorhexa.com/4dad4a.png";
                        case "Blue":
                            return "http://www.colorhexa.com/54a6d6.png";
                        case "Red":
                            return "http://www.colorhexa.com/ad4a54.png";
                        default:
                            return "http://www.colorhexa.com/ffffff.png";
                    }
                }
            }
        }
    }
}