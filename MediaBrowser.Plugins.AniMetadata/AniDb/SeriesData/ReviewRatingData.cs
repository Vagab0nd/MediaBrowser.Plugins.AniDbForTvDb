using System.Xml.Serialization;

namespace Emby.AniDbMetaStructure.AniDb.SeriesData
{
    public class ReviewRatingData : RatingData
    {
        [XmlIgnore]
        public override RatingType Type => RatingType.Review;
    }
}