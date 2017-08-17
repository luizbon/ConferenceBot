using System;

namespace ConferenceBot.Model
{
    [Serializable]
    public class Presenter: IEquatable<Presenter>
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string TwitterAlias { get; set; }
        public string[] Bio { get; set; }
        public string Website { get; set; }
        public string ImageUrl { get; set; }
        public string Tag { get; set; }

        public bool Equals(Presenter other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Presenter) obj);
        }

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : 0;
        }
    }
}