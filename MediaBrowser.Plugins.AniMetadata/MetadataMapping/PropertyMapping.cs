using System;
using System.Linq.Expressions;
using System.Reflection;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.MetadataMapping
{
    internal static class PropertyMapping<TSource, TTarget>
    {
        public static PropertyMapping<TSource, TTarget, TTargetProperty> Create<TTargetProperty>(
            Expression<Func<TTarget, TTargetProperty>> targetPropertySelector,
            Action<TSource, TTarget> map)
        {
            return new PropertyMapping<TSource, TTarget, TTargetProperty>(targetPropertySelector, map);
        }
    }

    /// <summary>
    ///     A mapping that sets a target metadata property based on a source property
    /// </summary>
    internal class PropertyMapping<TSource, TTarget, TTargetProperty>
        : IPropertyMapping<TSource, TTarget>
    {
        private readonly Action<TSource, TTarget> _map;
        
        public PropertyMapping(Expression<Func<TTarget, TTargetProperty>> targetPropertySelector,
            Action<TSource, TTarget> map)
        {
            var targetPropertyInfo = GetPropertyInfo(targetPropertySelector);

            TargetPropertyName = targetPropertyInfo.Name;

            _map = map;
        }

        public string TargetPropertyName { get; }

        public void Map(TSource source, TTarget target)
        {
            _map(source, target);
        }

        private PropertyInfo GetPropertyInfo<T, TProperty>(Expression<Func<T, TProperty>> propertySelector)
        {
            Option<MemberExpression> memberExpression = propertySelector.Body as MemberExpression;

            var propertyInfo = memberExpression.Map(m => m.Member as PropertyInfo);

            return propertyInfo.Match(m => m,
                () => throw new Exception($"{nameof(propertySelector)} is not a member expression"));
        }
    }
}