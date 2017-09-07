using System.Xml.Serialization;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData
{
    public class EpisodeRatingData
    {
        [XmlAttribute("votes")]
        public int VoteCount { get; set; }

        [XmlText]
        public float Rating { get; set; }
    }
}