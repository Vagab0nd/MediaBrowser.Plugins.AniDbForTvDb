using Emby.AniDbMetaStructure.AniList.Data;
using Emby.AniDbMetaStructure.Configuration;
using LanguageExt;

namespace Emby.AniDbMetaStructure.AniList
{
    internal interface IAniListNameSelector
    {
        Option<string> SelectTitle(AniListTitleData titleData, TitleType preferredTitleType,
            string metadataLanguage);

        Option<string> SelectName(AniListPersonNameData nameData, TitleType preferredTitleType,
            string metadataLanguage);
    }
}