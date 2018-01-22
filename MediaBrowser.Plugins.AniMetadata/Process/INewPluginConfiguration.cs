using MediaBrowser.Plugins.AniMetadata.Configuration;

namespace MediaBrowser.Plugins.AniMetadata.Process
{
    internal interface INewPluginConfiguration : IPluginConfiguration
    {
        /// <summary>
        ///     The source that was used to name the files
        /// </summary>
        ISource FileStructureSource { get; }

        /// <summary>
        ///     The source to use to structure the Emby library
        /// </summary>
        ISource LibraryStructureSource { get; }
    }
}