using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    public class EpisodeRating
    {
        [XmlAttribute("votes")]
        public int VoteCount { get; set; }

        [XmlText]
        public decimal Rating { get; set; }
    }
}