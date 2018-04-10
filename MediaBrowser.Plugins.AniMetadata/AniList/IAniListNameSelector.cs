using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.AniList.Data;
using MediaBrowser.Plugins.AniMetadata.Configuration;

namespace MediaBrowser.Plugins.AniMetadata.AniList
{
    internal interface IAniListNameSelector
    {
        Option<string> SelectTitle(AniListTitleData titleData, TitleType preferredTitleType,
            string metadataLanguage);

        Option<string> SelectName(AniListPersonNameData nameData, TitleType preferredTitleType,
            string metadataLanguage);
    }
}