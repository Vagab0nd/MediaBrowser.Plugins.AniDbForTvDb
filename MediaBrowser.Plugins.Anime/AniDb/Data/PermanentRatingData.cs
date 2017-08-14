using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    public class PermanentRatingData : RatingData
    {
        [XmlIgnore]
        public override RatingType Type => RatingType.Permanent;
    }
}