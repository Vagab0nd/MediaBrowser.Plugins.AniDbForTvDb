using System.Collections.Generic;
using LanguageExt;
using Newtonsoft.Json;

namespace MediaBrowser.Plugins.AniMetadata.AniList.Data
{
    internal class AniListSeriesData
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("series_type")]
        public AniListSeriesType SeriesType { get; set; }

        [JsonProperty("title_romaji")]
        public string TitleRomaji { get; set; }

        [JsonProperty("title_english")]
        public string TitleEnglish { get; set; }

        [JsonProperty("title_japanese")]
        public string TitleJapanese { get; set; }

        [JsonProperty("type")]
        public AniListMediaType Type { get; set; }

        [JsonProperty("start_date_fuzzy")]
        public Option<int> StartDateFuzzy { get; set; }

        [JsonProperty("end_date_fuzzy")]
        public Option<int> EndDateFuzzy { get; set; }

        [JsonProperty("season")]
        public Option<int> Season { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("synonyms")]
        public IEnumerable<string> Synonyms { get; set; }

        [JsonProperty("genres")]
        public IEnumerable<string> Genres { get; set; }

        [JsonProperty("average_score")]
        public double AverageScore { get; set; }

        [JsonProperty("popularity")]
        public int Popularity { get; set; }

        [JsonProperty("title_romaji")]
        public string ImageUrlSmall { get; set; }

        [JsonProperty("image_url_sml")]
        public string ImageUrlMedium { get; set; }

        [JsonProperty("image_url_lge")]
        public string ImageUrlLarge { get; set; }

        [JsonProperty("image_url_banner")]
        public string ImageUrlBanner { get; set; }

        [JsonProperty("updated_at")]
        public int UpdatedAt { get; set; }

        [JsonProperty("total_episodes")]
        public int TotalEpisodes { get; set; }

        [JsonProperty("duration")]
        public Option<int> Duration { get; set; }

        [JsonProperty("airing_status")]
        public AniListAiringStatus AiringStatus { get; set; }
    }
}