using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Mapping.Data
{
    /// <summary>
    ///     A list of anime mappings between AniDb, theTVDB, and themoviedb
    /// </summary>
    [XmlType(AnonymousType = true)]
    [XmlRoot("anime-list", Namespace = "", IsNullable = false)]
    public class AnimeMappingListData
    {
        /// <remarks />
        [XmlElement("anime")]
        public AniDbSeriesMappingData[] AnimeSeriesMapping { get; set; }
    }
}