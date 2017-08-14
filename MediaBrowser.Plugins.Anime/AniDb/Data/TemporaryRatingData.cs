using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    public class TemporaryRatingData : RatingData
    {
        [XmlIgnore]
        public override RatingType Type => RatingType.Temporary;
    }
}