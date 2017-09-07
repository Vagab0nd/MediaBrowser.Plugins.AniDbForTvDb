using System.Xml.Serialization;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData
{
    public class TemporaryRatingData : RatingData
    {
        [XmlIgnore]
        public override RatingType Type => RatingType.Temporary;
    }
}