using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    public class AniDbEpisode
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("update")]
        public DateTime LastUpdated { get; set; }

        [XmlElement("epno")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public EpisodeNumber RawEpisodeNumber { get; set; }

        [XmlIgnore]
        public IAniDbEpisodeNumber EpisodeNumber => RawEpisodeNumber;

        [XmlElement("length")]
        public int TotalMinutes { get; set; }

        [XmlElement("airdate")]
        public DateTime AirDate { get; set; }

        [XmlElement("rating")]
        public EpisodeRating Rating { get; set; }

        [XmlElement("title", typeof(EpisodeTitle))]
        public EpisodeTitle[] Titles { get; set; }

        [XmlElement("summary")]
        public string Summary { get; set; }
    }
}