using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    public enum Gender
    {
        [XmlEnum("male")]
        Male,
        [XmlEnum("female")]
        Female,
        [XmlEnum("unknown")]
        Unknown
    }
}