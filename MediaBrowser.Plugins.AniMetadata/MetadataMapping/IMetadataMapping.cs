using System.Collections.Generic;

namespace MediaBrowser.Plugins.AniMetadata.MetadataMapping
{
    public interface IMetadataMapping
    {
        TMetadata Apply<TMetadata>(object source, TMetadata target);

        TMetadata Apply<TMetadata>(IEnumerable<object> sources, TMetadata target);
    }
}