using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.SeriesData
{
    public class EpisodeTitle
    {
        [XmlAttribute("xml:lang")]
        public string Language { get; set; }
        
        [XmlText]
        public string Title { get; set; }
    }
}