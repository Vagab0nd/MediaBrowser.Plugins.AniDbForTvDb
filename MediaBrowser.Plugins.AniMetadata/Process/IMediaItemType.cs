﻿using Emby.AniDbMetaStructure.Configuration;
using LanguageExt;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Logging;

namespace Emby.AniDbMetaStructure.Process
{
    public interface IMediaItemType
    {
    }

    internal interface IMediaItemType<TEmbyItem> : IMediaItemType where TEmbyItem : BaseItem
    {
        Either<ProcessFailedResult, IMetadataFoundResult<TEmbyItem>> CreateMetadataFoundResult(
            IPluginConfiguration pluginConfiguration, IMediaItem mediaItem, ILogManager logManager);
    }
}