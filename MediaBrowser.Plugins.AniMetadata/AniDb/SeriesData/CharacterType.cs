using System.Xml.Serialization;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData
{
    public class CharacterType
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        /// <summary>
        ///     The type of the character, e.g. 'Character', 'Vessel', 'Organization'
        /// </summary>
        [XmlText]
        public string Name { get; set; }
    }
}