using System.Xml.Serialization;

namespace Emby.AniDbMetaStructure.AniDb.SeriesData
{
    public class EpisodeRatingData
    {
        [XmlAttribute("votes")]
        public int VoteCount { get; set; }

        [XmlText]
        public float Rating { get; set; }
    }
}