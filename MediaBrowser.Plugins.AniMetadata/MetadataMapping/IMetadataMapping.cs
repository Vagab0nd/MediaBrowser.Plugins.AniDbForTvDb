using System.Collections.Generic;

namespace MediaBrowser.Plugins.AniMetadata.MetadataMapping
{
    internal interface IMetadataMapping
    {
        TMetadata Apply<TMetadata>(object source, TMetadata target);

        TMetadata Apply<TMetadata>(IEnumerable<object> sources, TMetadata target);
    }
}