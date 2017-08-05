using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.SeriesData
{
    [XmlType(AnonymousType = true)]
    public abstract class Rating
    {
        [XmlAttribute("count")]
        public int Count { get; set; }

        [XmlText]
        public decimal Value { get; set; }

        public abstract RatingType Type { get; }
    }
}