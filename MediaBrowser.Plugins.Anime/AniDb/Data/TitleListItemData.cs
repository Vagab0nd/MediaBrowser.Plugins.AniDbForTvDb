using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
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