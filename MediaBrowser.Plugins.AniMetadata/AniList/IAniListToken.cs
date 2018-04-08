using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.AniList
{
    internal interface IAniListToken
    {
        OptionAsync<string> GetToken();
    }
}