namespace MediaBrowser.Plugins.AniMetadata.AniList.Data
{
    internal class AniListGraphQlPage<T>
    {
        public AniListGraphQlPage(T page)
        {
            Page = page;
        }

        public T Page { get; }
    }
}