using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Series.Data
{
    public class CharacterRatingData
    {
        [XmlAttribute("votes")]
        public int VoteCount { get; set; }

        [XmlText]
        public float Value { get; set; }
    }
}