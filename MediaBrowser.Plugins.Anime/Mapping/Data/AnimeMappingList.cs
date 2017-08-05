using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.Mapping.Data
{
    /// <summary>
    /// A list of anime mappings between AniDb, theTVDB, and themoviedb
    /// </summary>
    [XmlType(AnonymousType = true)]
    [XmlRoot("anime-list", Namespace = "", IsNullable = false)]
    public class AnimeMappingList
    {
        /// <remarks />
        [XmlElement("anime")]
        public AnimeSeriesMapping[] AnimeSeriesMapping { get; set; }
    }
}