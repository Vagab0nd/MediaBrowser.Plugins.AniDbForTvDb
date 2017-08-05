using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    public class EpisodeTitle
    {
        [XmlAttribute("xml:lang")]
        public string Language { get; set; }
        
        [XmlText]
        public string Title { get; set; }
    }
}