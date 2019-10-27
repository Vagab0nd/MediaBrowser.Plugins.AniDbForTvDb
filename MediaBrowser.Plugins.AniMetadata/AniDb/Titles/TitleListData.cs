using System.Xml.Serialization;

namespace Emby.AniDbMetaStructure.AniDb.Titles
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("animetitles", Namespace = "", IsNullable = false)]
    public class TitleListData
    {
        [XmlElement("anime", typeof(TitleListItemData))]
        public TitleListItemData[] Titles { get; set; }
    }
}