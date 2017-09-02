using System.Xml.Serialization;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.Series.Data
{
    public class PermanentRatingData : RatingData
    {
        [XmlIgnore]
        public override RatingType Type => RatingType.Permanent;
    }
}