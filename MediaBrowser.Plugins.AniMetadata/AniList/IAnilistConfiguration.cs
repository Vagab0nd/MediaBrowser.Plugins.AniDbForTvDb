using LanguageExt;

namespace Emby.AniDbMetaStructure.AniList
{
    internal interface IAnilistConfiguration
    {
        bool IsLinked { get; }

        string AuthorizationCode { get; }

        Option<string> AccessToken { get; set; }
    }
}