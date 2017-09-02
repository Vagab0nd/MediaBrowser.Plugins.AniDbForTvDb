using System.Xml.Serialization;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Seiyuu
{
    [XmlRoot("seiyuulist")]
    public class SeiyuuListData
    {
        [XmlElement("seiyuu")]
        public SeiyuuData[] Seiyuu { get; set; }
    }
}