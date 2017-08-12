using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    [XmlRoot("seiyuulist")]
    public class SeiyuuList
    {
        [XmlElement("seiyuu")]
        public Seiyuu[] Seiyuu { get; set; }
    }
}