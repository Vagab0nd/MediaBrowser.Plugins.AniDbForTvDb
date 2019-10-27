using System.Xml.Serialization;

namespace Emby.AniDbMetaStructure.AniDb.SeriesData
{
    public class PermanentRatingData : RatingData
    {
        [XmlIgnore]
        public override RatingType Type => RatingType.Permanent;
    }
}