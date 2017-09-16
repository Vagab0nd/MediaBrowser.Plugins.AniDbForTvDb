using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Plugins.AniMetadata.MetadataMapping
{
    internal interface IPropertyMapping<in TSource, in TTarget>
    {
        string TargetPropertyName { get; }

        void Map(TSource source, TTarget target);
    }
}