using System.Xml.Serialization;

namespace MediaBrowser.Plugins.AniMetadata.AniDb.SeriesData
{
    public class EpisodeNumberData : IAniDbEpisodeNumber
    {
        /// <summary>
        ///     1 = Normal episode,
        ///     2 = Special,
        ///     3 = Op / Ed,
        ///     5 = Parody / Fandub
        /// </summary>
        [XmlAttribute("type")]
        public int RawType { get; set; }

        /// <summary>
        ///     Where X is an integer:
        ///     X for a normal episode,
        ///     SX for a special episode,
        ///     CX for an op / ed,
        ///     PX for a parody / fandub
        /// </summary>
        [XmlText]
        public string RawNumber { get; set; }

        public int Number => int.Parse(RemoveEpisodeNumberPrefix(RawNumber));

        public EpisodeType Type => (EpisodeType)RawType;

        private string RemoveEpisodeNumberPrefix(string episodeNumber)
        {
            episodeNumber = episodeNumber ?? "-1";

            switch (Type)
            {
                case EpisodeType.Special:
                    return episodeNumber.TrimStart('S');
                case EpisodeType.OpEd:
                    return episodeNumber.TrimStart('C');
                case EpisodeType.ParodyOrFanDub:
                    return episodeNumber.TrimStart('P');
                default:
                    return episodeNumber;
            }
        }
    }
}