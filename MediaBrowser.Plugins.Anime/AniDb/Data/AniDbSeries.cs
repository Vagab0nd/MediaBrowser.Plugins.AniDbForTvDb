using System;
using System.Xml.Serialization;

namespace MediaBrowser.Plugins.Anime.AniDb.Data
{
    [XmlType(AnonymousType = true)]
    [XmlRoot("anime", Namespace = "", IsNullable = false)]
    public class AniDbSeries
    {
        [XmlAttribute("id")]
        public int Id { get; set; }

        [XmlAttribute("restricted")]
        public bool Restricted { get; set; }

        [XmlElement("type")]
        public string Type { get; set; }

        [XmlElement("episodecount")]
        public int EpisodeCount { get; set; }

        [XmlElement("startdate")]
        public DateTime? StartDate { get; set; }

        [XmlElement("enddate")]
        public DateTime? EndDate { get; set; }

        [XmlArray("titles")]
        [XmlArrayItem("title", typeof(ItemTitle))]
        public ItemTitle[] Titles { get; set; }

        [XmlArray("relatedanime")]
        [XmlArrayItem("anime", typeof(RelatedSeries))]
        public RelatedSeries[] RelatedSeries { get; set; }

        [XmlArray("similaranime")]
        [XmlArrayItem("anime", typeof(SimilarSeries))]
        public SimilarSeries[] SimilarSeries { get; set; }

        [XmlElement("url")]
        public string Url { get; set; }

        [XmlArray("creators")]
        [XmlArrayItem("name", typeof(Creator))]
        public Creator[] Creators { get; set; } = new Creator[0];

        /// <summary>
        ///     Contains links in the form url [link text], e.g. 'http://anidb.net/cr4495 [Morioka Hiroyuki]'
        /// </summary>
        [XmlElement("description")]
        public string Description { get; set; }

        [XmlArray("ratings")]
        [XmlArrayItem("permanent", typeof(PermanentRating))]
        [XmlArrayItem("temporary", typeof(TemporaryRating))]
        [XmlArrayItem("review", typeof(ReviewRating))]
        public Rating[] Ratings { get; set; }

        [XmlElement("picture")]
        public string PictureFileName { get; set; }

        [XmlArray("tags")]
        [XmlArrayItem("tag", typeof(Tag))]
        public Tag[] Tags { get; set; }

        [XmlArray("episodes")]
        [XmlArrayItem("episode", typeof(AniDbEpisode))]
        public AniDbEpisode[] Episodes { get; set; }

        [XmlArray("characters")]
        [XmlArrayItem("character", typeof(Character))]
        public Character[] Characters { get; set; } = new Character[0];
    }
}