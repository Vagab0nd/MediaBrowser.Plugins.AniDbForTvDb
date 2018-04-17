using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.AniList
{
    internal interface IAnilistConfiguration
    {
        bool IsLinked { get; }

        string AuthorisationCode { get; }

        Option<string> AccessToken { get; set; }
    }
}