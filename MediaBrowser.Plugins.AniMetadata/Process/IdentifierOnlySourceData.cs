using System;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal class IdentifierOnlySourceData : ISourceData
    {
        public IdentifierOnlySourceData(ISource source, Option<int> id, IItemIdentifier identifier)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Id = id;
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        }

        public ISource Source { get; }

        public Option<int> Id { get; }

        public IItemIdentifier Identifier { get; }

        public object Data => null;

        public Option<TData> GetData<TData>() where TData : class
        {
            return Option<TData>.None;
        }
    }
}