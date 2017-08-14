using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    public class ReviewRatingData : RatingData
    {
        [XmlIgnore]
        public override RatingType Type => RatingType.Review;
    }
}