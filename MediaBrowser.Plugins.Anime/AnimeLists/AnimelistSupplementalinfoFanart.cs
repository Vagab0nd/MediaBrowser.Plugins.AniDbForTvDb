using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AnimeLists
{
    /// <remarks />
    [XmlType(AnonymousType = true)]
    public class AnimelistSupplementalinfoFanart
    {
        /// <remarks />
        [XmlElement("thumb")]
        public AnimelistSupplementalinfoFanartThumb Thumb { get; set; }
    }
}