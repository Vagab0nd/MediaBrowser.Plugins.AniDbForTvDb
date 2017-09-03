using System;
using System.Linq;
using System.Reflection;
using LanguageExt;
using LanguageExt.UnsafeValueAccess;
using Newtonsoft.Json;

namespace MediaBrowser.Plugins.AniMetadata
{
    internal class MaybeJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var wrappedType = value.GetType().GetGenericArguments()[0];

            var hasValue = (bool)value.GetType().GetProperty("IsSome").GetValue(value);

            if (hasValue)
            {
                var methods = typeof(UnsafeValueAccessExtensions).GetMethods().Where(m => m.Name == nameof(UnsafeValueAccessExtensions.Value) && m.GetGenericArguments().Length == 1).ToList();

                var method = methods.First().MakeGenericMethod(wrappedType);

                writer.WriteValue(method.Invoke(null, new[] { value }));
            }
            else
            {
                Type nullableType;

                if (wrappedType.IsValueType)
                {
                    nullableType = typeof(Nullable<>).MakeGenericType(wrappedType);
                }
                else
                {
                    nullableType = wrappedType;
                }

                writer.WriteValue(Activator.CreateInstance(nullableType));
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            if (reader.Value == null)
            {
                return objectType.GetField(nameof(Option<object>.None)).GetValue(null);
            }

            var wrappedType = objectType.GetGenericArguments()[0];
            var castValue = Convert.ChangeType(reader.Value, wrappedType);

            return objectType.GetMethod("Some", BindingFlags.Public | BindingFlags.Static).Invoke(null, new[] { castValue });
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsConstructedGenericType && objectType.GetGenericTypeDefinition() == typeof(Option<>);
        }
    }
}