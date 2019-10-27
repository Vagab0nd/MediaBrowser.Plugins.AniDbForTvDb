namespace Emby.AniDbMetaStructure.AniDb.Titles
{
    internal interface ITitleNormaliser
    {
        string GetNormalisedTitle(string title);
    }
}