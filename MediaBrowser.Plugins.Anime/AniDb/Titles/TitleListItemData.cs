using System.Xml.Serialization;
using MediaBrowser.Plugins.Anime.AniDb.Series;
using MediaBrowser.Plugins.Anime.AniDb.Series.Data;

namespace MediaBrowser.Plugins.Anime.AniDb.Titles
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