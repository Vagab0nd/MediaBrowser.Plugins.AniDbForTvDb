using System.Xml.Serialization;
using Emby.AniDbMetaStructure.AniDb.SeriesData;

namespace Emby.AniDbMetaStructure.AniDb.Titles
{
    [XmlType(AnonymousType = true)]
    public class TitleListItemData
    {
        [XmlAttribute("aid")]
        public int AniDbId { get; set; }

        [XmlElement("title", typeof(ItemTitleData))]
        public ItemTitleData[] Titles { get; set; }
    }
}