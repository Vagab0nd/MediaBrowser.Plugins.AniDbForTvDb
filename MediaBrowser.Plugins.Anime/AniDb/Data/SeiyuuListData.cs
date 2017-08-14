using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    [XmlRoot("seiyuulist")]
    public class SeiyuuListData
    {
        [XmlElement("seiyuu")]
        public SeiyuuData[] Seiyuu { get; set; }
    }
}