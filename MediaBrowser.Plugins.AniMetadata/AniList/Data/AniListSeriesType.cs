using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Emby.AniDbMetaStructure.AniList.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum AniListSeriesType
    {
        Anime,
        Manga
    }
}