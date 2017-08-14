using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("animetitles", Namespace = "", IsNullable = false)]
    public class TitleListData
    {
        [XmlElement("anime", typeof(TitleListItemData))]
        public TitleListItemData[] Titles { get; set; }
    }
}