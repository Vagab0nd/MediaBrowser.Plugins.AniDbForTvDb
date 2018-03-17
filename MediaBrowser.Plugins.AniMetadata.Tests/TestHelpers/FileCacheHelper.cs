using System;
using System.IO;
using NUnit.Framework;

namespace MediaBrowser.Plugins.AniMetadata.Tests.TestHelpers
{
    internal static class FileCacheHelper
    {
        public static void SetupCachedFile(string cachePath, string relativeSourcePath, string relativeTargetPath)
        {
            Directory.GetParent(cachePath + relativeTargetPath).Create();

            File.Copy(TestContext.CurrentContext.WorkDirectory + "\\TestData\\" + relativeSourcePath,
                cachePath + relativeTargetPath);

            File.SetLastWriteTime(cachePath + relativeTargetPath, DateTime.Now);
        }
    }
}