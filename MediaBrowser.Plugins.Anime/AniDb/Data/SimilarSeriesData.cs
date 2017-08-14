using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    [XmlType(AnonymousType = true)]
    public class SimilarSeriesData
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("approval")]
        public int Approval { get; set; }

        [XmlAttribute("total")]
        public int Total { get; set; }

        [XmlText]
        public string Title { get; set; }
    }
}