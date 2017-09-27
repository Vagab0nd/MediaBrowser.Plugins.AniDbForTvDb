using LanguageExt;
using MediaBrowser.Plugins.AniMetadata.PropertyMapping;

namespace MediaBrowser.Plugins.AniMetadata.Configuration
{
    internal class NullMapping : IPropertyMapping
    {
        public NullMapping(string targetPropertyName)
        {
            TargetPropertyName = targetPropertyName;
        }

        public string TargetPropertyName { get; }

        public string SourceName => "None";

        public bool CanApply(object source, object target)
        {
            return true;
        }

        public Option<TTarget> Apply<TTarget>(object source, TTarget target)
        {
            return target;
        }
    }
}