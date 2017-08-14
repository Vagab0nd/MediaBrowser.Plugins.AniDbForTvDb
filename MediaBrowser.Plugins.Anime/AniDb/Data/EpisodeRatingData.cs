using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    public class EpisodeRatingData
    {
        [XmlAttribute("votes")]
        public int VoteCount { get; set; }

        [XmlText]
        public float Rating { get; set; }
    }
}