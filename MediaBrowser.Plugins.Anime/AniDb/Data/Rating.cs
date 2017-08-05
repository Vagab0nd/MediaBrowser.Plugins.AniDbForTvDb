using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    [XmlType(AnonymousType = true)]
    public abstract class Rating
    {
        [XmlAttribute("count")]
        public int Count { get; set; }

        [XmlText]
        public float Value { get; set; }

        public abstract RatingType Type { get; }
    }
}