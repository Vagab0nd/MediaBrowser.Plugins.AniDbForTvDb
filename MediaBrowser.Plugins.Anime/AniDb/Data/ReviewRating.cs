using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    public class ReviewRating : Rating
    {
        [XmlIgnore]
        public override RatingType Type => RatingType.Review;
    }
}