using System.Xml.Serialization;

namespace Emby.AniDbMetaStructure.AniDb.Seiyuu
{
    [XmlRoot("seiyuulist")]
    public class SeiyuuListData
    {
        [XmlElement("seiyuu")]
        public SeiyuuData[] Seiyuu { get; set; }
    }
}