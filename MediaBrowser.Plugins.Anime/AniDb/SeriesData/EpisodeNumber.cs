using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.SeriesData
{
    public class EpisodeNumber
    {
        [XmlAttribute("type")]
        public int Type { get; set; }

        [XmlText]
        public string Number { get; set; }
    }
}