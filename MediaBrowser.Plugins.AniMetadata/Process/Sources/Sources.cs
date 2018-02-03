using System.Collections.Generic;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Process.Sources
{
    internal class Sources : ISources
    {
        private readonly IEnumerable<ISource> _sources;

        public Sources(IEnumerable<ISource> sources)
        {
            _sources = sources;
        }

        public Option<ISource> GetSource<TSource>() where TSource : ISource
        {
            return _sources.Filter(s => s is TSource).ToOption();
        }
    }
}