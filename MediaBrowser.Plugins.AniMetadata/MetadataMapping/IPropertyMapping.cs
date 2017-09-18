using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.MetadataMapping
{
    public interface IPropertyMapping
    {
        string TargetPropertyName { get; }

        string SourceName { get; }

        bool CanReadFrom<TSource>(TSource source);

        Option<TTarget> Apply<TTarget>(object source, TTarget target);
    }
}