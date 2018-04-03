using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MediaBrowser.Plugins.AniMetadata.AniList.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum AniListSeriesType
    {
        Anime,
        Manga
    }
}