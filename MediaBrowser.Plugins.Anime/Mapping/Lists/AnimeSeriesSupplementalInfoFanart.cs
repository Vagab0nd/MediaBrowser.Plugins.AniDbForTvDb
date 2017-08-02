using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.Mapping.Lists
{
    /// <summary>
    ///     Details of additional Fanart specified as supplemental information
    /// </summary>
    [XmlType(AnonymousType = true)]
    public class AnimeSeriesSupplementalInfoFanart
    {
        /// <summary>
        ///     Details of the thumbnail fanart
        /// </summary>
        [XmlElement("thumb")]
        public AnimeSeriesSupplementalInfoFanartThumbnail Thumbnail { get; set; }
    }
}