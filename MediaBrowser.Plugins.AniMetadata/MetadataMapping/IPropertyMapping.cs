using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.MetadataMapping
{
    public interface IPropertyMapping
    {
        string TargetPropertyName { get; }

        string SourceName { get; }

        bool CanApply(object source, object target);

        Option<TTarget> Apply<TTarget>(object source, TTarget target);
    }
}