using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.SeriesData
{
    [XmlType(AnonymousType = true)]
    public class SeriesTitle
    {
        [XmlAttribute("xml:lang")]
        public string Language { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlText]
        public string Title { get; set; }
    }
}