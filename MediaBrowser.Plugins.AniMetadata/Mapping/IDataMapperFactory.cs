using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    internal interface IDataMapperFactory
    {
        OptionAsync<IDataMapper> GetDataMapperAsync();
    }
}