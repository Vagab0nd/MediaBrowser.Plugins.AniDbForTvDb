using System.Xml.Serialization;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData
{
    [XmlType(AnonymousType = true)]
    public class RelatedSeriesData
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlText]
        public string Title { get; set; }
    }
}