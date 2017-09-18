using System;
using System.Linq.Expressions;
using System.Reflection;
using LanguageExt;

namespace MediaBrowser.Plugins.AniMetadata.MetadataMapping
{
    internal static class PropertyMapping<TSource, TTarget> where TSource : class where TTarget : class
    {
        public static PropertyMapping<TSource, TTarget, TTargetProperty> Create<TTargetProperty>(
            Expression<Func<TTarget, TTargetProperty>> targetPropertySelector,
            Action<TSource, TTarget> map)
        {
            return new PropertyMapping<TSource, TTarget, TTargetProperty>(targetPropertySelector, map, "");
        }
    }

    /// <summary>
    ///     A mapping that sets a target metadata property based on a source property
    /// </summary>
    internal class PropertyMapping<TSource, TTarget, TTargetProperty> : IPropertyMapping
        where TSource : class where TTarget : class
    {
        private readonly Action<TSource, TTarget> _apply;

        public PropertyMapping(Expression<Func<TTarget, TTargetProperty>> targetPropertySelector,
            Action<TSource, TTarget> apply, string sourceName)
        {
            var targetPropertyInfo = GetPropertyInfo(targetPropertySelector);

            TargetPropertyName = targetPropertyInfo.Name;
            SourceName = sourceName;

            _apply = apply;
        }

        public string TargetPropertyName { get; }

        public string SourceName { get; }

        public bool CanReadFrom<T>(T source)
        {
            return typeof(T).IsAssignableFrom(typeof(TSource));
        }

        public Option<TT> Apply<TT>(object source, TT target)
        {
            Option<TSource> typedSource = source as TSource;
            Option<TTarget> typedTarget = target as TTarget;

            return typedSource.Bind(s => typedTarget.Map(t =>
            {
                _apply(s, t);
                return target;
            }));
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