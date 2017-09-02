using System.Collections.Generic;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Plugins.AniMetadata.Configuration;

namespace MediaBrowser.Plugins.AniMetadata
{
    public class Plugin
        : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer) : base(
            applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public override string Name => "AniMetadata";

        public static Plugin Instance { get; private set; }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "AniMetadata",
                    EmbeddedResourcePath = "MediaBrowser.Plugins.AniMetadata.Configuration.configPage.html"
                }
            };
        }
    }
}