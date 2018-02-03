using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal interface ISources
    {
        Option<ISource> GetSource<TSource>() where TSource : ISource;
    }
}