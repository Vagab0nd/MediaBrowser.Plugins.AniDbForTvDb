using System.Xml.Serialization;
using MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Titles
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