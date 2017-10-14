namespace MediaBrowser.Plugins.AniMetadata.Mapping
{
    internal interface IDataMapperFactory
    {
        IDataMapper GetDataMapper(IMappingList mappingList);
    }
}