using System.Xml.Serialization;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData
{
    public class ReviewRatingData : RatingData
    {
        [XmlIgnore]
        public override RatingType Type => RatingType.Review;
    }
}