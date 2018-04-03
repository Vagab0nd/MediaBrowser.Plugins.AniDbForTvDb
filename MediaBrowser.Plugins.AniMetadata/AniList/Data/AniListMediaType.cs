using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MediaBrowser.Plugins.AniMetadata.AniList.Data
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum AniListMediaType
    {
        [EnumMember(Value = "TV")] Tv = 0,

        [EnumMember(Value = "TV Short")] TvShort = 1,

        [EnumMember(Value = "Movie")] Movie = 2,

        [EnumMember(Value = "Special")] Special = 3,

        [EnumMember(Value = "OVA")] Ova = 4,

        [EnumMember(Value = "ONA")] Ona = 5,

        [EnumMember(Value = "Music")] Music = 6,

        [EnumMember(Value = "Manga")] Manga = 7,

        [EnumMember(Value = "Novel")] Novel = 8,

        [EnumMember(Value = "One Shot")] OneShot = 9,

        [EnumMember(Value = "Doujin")] Doujin = 10,

        [EnumMember(Value = "Manhua")] Manhua = 11,

        [EnumMember(Value = "Manhwa")] Manhwa = 12
    }
}