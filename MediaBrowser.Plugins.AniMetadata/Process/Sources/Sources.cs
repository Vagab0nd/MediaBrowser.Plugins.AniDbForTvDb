using System;
using System.Collections.Generic;
using System.Linq;

namespace MediaBrowser.Plugins.AniMetadata.Process.Sources
{
    internal class Sources : ISources
    {
        private readonly Lazy<IEnumerable<ISource>> _sources;
        private readonly Lazy<IAniDbSource> _aniDbSource;
        private readonly Lazy<ITvDbSource> _tvDbSource;
        private readonly Lazy<IAniListSource> _aniListSource;

        public Sources(Func<IAniDbSource> aniDbSource, Func<ITvDbSource> tvDbSource, Func<IAniListSource> aniListSource, Func<IEnumerable<ISource>> sources)
        {
            _aniListSource = new Lazy<IAniListSource>(aniListSource);
            _sources = new Lazy<IEnumerable<ISource>>(sources);
            _aniDbSource = new Lazy<IAniDbSource>(aniDbSource);
            _tvDbSource = new Lazy<ITvDbSource>(tvDbSource);
        }

        public IAniDbSource AniDb => _aniDbSource.Value;

        public ITvDbSource TvDb => _tvDbSource.Value;

        public IAniListSource AniList => _aniListSource.Value;

        public ISource Get(string sourceName)
        {
            return _sources.Value.Single(s => s.Name == sourceName);
        }
    }
}