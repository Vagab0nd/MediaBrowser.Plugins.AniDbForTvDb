using System.Xml.Serialization;

namespace Emby.AniDbMetaStructure.AniDb.SeriesData
{
    public class TemporaryRatingData : RatingData
    {
        [XmlIgnore]
        public override RatingType Type => RatingType.Temporary;
    }
}