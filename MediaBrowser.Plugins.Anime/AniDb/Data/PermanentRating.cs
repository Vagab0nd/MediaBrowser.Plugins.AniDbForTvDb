using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    public class PermanentRating : Rating
    {
        [XmlIgnore]
        public override RatingType Type => RatingType.Permanent;
    }
}