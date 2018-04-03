using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MediaBrowser.Plugins.AniMetadata.AniList.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum AniListAiringStatus
    {
        [EnumMember(Value = "finished airing")]
        Finished,

        [EnumMember(Value = "currently airing")]
        Airing,
        [EnumMember(Value = "not yet aired")] NotYetAired,
        [EnumMember(Value = "cancelled")] Cancelled
    }
}