using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("animetitles", Namespace = "", IsNullable = false)]
    public class AniDbTitleList
    {
        [XmlElement("anime", typeof(TitleListItem))]
        public TitleListItem[] Titles { get; set; }
    }
}
