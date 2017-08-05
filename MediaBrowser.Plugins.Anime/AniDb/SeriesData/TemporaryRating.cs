using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.SeriesData
{
    public class TemporaryRating : Rating
    {
        [XmlIgnore]
        public override RatingType Type => RatingType.Temporary;
    }
}