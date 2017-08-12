using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    [XmlType(AnonymousType = true)]
    public abstract class Rating
    {
        [XmlAttribute("count")]
        public int VoteCount { get; set; }

        [XmlText]
        public float Value { get; set; }

        public abstract RatingType Type { get; }
    }
}