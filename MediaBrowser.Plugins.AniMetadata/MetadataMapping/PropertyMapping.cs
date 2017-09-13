using System;
using System.Linq.Expressions;
using System.Reflection;
using MediaBrowser.Controller.Entities;

namespace MediaBrowser.Plugins.AniMetadata.MetadataMapping
{
    /// <summary>
    ///     A mapping between a source property and a metadata property
    /// </summary>
    internal class PropertyMapping<TSource, TTarget, TSourceProperty, TTargetProperty> : IPropertyMapping<TSource, TTarget> where TTarget : BaseItem
    {
        private readonly PropertyInfo _sourceProperty;
        private readonly PropertyInfo _targetProperty;

        public PropertyMapping(Expression<Func<TSource, TSourceProperty>> sourcePropertySelector,
            Expression<Func<TTarget, TTargetProperty>> targetPropertySelector)
        {
            _sourceProperty = GetPropertyInfo(sourcePropertySelector);
            _targetProperty = GetPropertyInfo(targetPropertySelector);
        }

        public string SourcePropertyName => _sourceProperty.Name;

        public string TargetPropertyName => _targetProperty.Name;

        public void Map(TSource source, TTarget target)
        {
            var sourceValue = _sourceProperty.GetValue(source);

            _targetProperty.SetValue(target, sourceValue);
        }

        private PropertyInfo GetPropertyInfo<T, TProperty>(Expression<Func<T, TProperty>> propertySelector)
        {
            var memberExpression = propertySelector.Body as MemberExpression;
            
            return memberExpression.Member as PropertyInfo;
        }
    }
}