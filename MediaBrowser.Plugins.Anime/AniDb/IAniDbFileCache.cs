﻿using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MediaBrowser.Plugins.Anime.AniDb
{
    internal interface IAniDbFileCache
    {
        Task<FileInfo> GetFileAsync(IAniDbFileSpec fileSpec, CancellationToken cancellationToken);
    }
}