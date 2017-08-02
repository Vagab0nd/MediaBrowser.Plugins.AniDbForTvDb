using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AnimeLists
{
    /// <remarks />
    [XmlType(IncludeInSchema = false)]
    public enum ItemsChoiceType
    {
        /// <remarks />
        credits,

        /// <remarks />
        director,

        /// <remarks />
        fanart,

        /// <remarks />
        genre,

        /// <remarks />
        studio
    }
}