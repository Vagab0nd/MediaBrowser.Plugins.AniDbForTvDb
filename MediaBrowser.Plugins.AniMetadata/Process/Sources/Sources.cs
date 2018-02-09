using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Plugins.AniMetadata.Process.Sources
{
    internal class Sources : ISources
    {
        private readonly Lazy<IEnumerable<ISource>> _sources;
        private readonly Lazy<ISource> _aniDbSource;
        private readonly Lazy<ISource> _tvDbSource;

        public Sources(Func<AniDbSource> aniDbSource, Func<TvDbSource> tvDbSource, Func<IEnumerable<ISource>> sources)
        {
            _sources = new Lazy<IEnumerable<ISource>>(sources);
            _aniDbSource = new Lazy<ISource>(aniDbSource);
            _tvDbSource = new Lazy<ISource>(tvDbSource);
        }

        public ISource AniDb => _aniDbSource.Value;

        public ISource TvDb => _tvDbSource.Value;

        public ISource Get(string sourceName)
        {
            return _sources.Value.Single(s => s.Name == sourceName);
        }

        public IEnumerable<ISource> GetAll()
        {
            return _sources.Value;
        }
    }
}