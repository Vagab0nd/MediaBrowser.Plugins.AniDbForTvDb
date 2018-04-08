using System.Collections.Generic;

namespace MediaBrowser.Plugins.AniMetadata.AniList.Data
{
    internal class GraphQlEdge<T>
    {
        public GraphQlEdge(IEnumerable<T> edges)
        {
            Edges = edges;
        }

        public IEnumerable<T> Edges { get; }
    }
}