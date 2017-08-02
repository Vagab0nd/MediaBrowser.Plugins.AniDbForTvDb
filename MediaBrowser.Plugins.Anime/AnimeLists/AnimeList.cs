using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AnimeLists
{
    /// <remarks />
    [XmlType(AnonymousType = true)]
    [XmlRoot("anime-list", Namespace = "", IsNullable = false)]
    public class Animelist
    {
        /// <remarks />
        [XmlElement("anime")]
        public AnimelistAnime[] Anime { get; set; }
    }
}