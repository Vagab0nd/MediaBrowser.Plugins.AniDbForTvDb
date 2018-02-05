using System;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class SourceData<TData> : ISourceData where TData : class
    {
        private readonly TData _data;

        public SourceData(ISource source, Option<int> id, IItemIdentifier identifier, TData data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            Id = id;
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        }

        public ISource Source { get; }

        public Option<int> Id { get; }

        public IItemIdentifier Identifier { get; }

        public TData Data => _data;

        Option<TRequestedData> ISourceData.GetData<TRequestedData>()
        {
            return _data as TRequestedData;
        }
    }
}