using System;
using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    public class Episode
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("update")]
        public DateTime LastUpdated { get; set; }

        [XmlElement("epno")]
        public EpisodeNumber EpisodeNumber { get; set; }

        [XmlElement("length")]
        public int TotalMinutes { get; set; }

        [XmlElement("airdate")]
        public DateTime AirDate { get; set; }

        [XmlElement("rating")]
        public EpisodeRating Rating { get; set; }

        [XmlElement("title", typeof(EpisodeTitle))]
        public EpisodeTitle[] Titles { get; set; }
    }
}