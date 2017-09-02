using System.Xml.Serialization;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Series.Data
{
    [XmlType(AnonymousType = true)]
    public class CreatorData
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlText]
        public string Name { get; set; }
    }
}