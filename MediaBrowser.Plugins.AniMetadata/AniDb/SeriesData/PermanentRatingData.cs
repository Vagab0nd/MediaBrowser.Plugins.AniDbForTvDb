using System.Xml.Serialization;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData
{
    public class PermanentRatingData : RatingData
    {
        [XmlIgnore]
        public override RatingType Type => RatingType.Permanent;
    }
}