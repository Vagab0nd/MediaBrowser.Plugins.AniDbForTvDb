using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    public class TemporaryRating : Rating
    {
        [XmlIgnore]
        public override RatingType Type => RatingType.Temporary;
    }
}