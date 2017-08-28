using System;
using System.Linq;
using System.Reflection;
using Functional.Maybe;
using Newtonsoft.Json;

namespace MediaBrowser.Plugins.Anime
{
    internal class MaybeJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //serializer.Serialize(writer, value);
            //return;

            var wrappedType = value.GetType().GetGenericArguments()[0];

            var hasValue = (bool)value.GetType().GetProperty("HasValue").GetValue(value);

            if (hasValue)
            {
                writer.WriteValue(value.GetType().GetProperty("Value").GetValue(value));
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
                return objectType.GetField("Nothing").GetValue(null);
            }

            var wrappedType = objectType.GetGenericArguments()[0];
            var castValue = Convert.ChangeType(reader.Value, wrappedType);

            return typeof(MaybeConvertions).GetMethods(BindingFlags.Public | BindingFlags.Static).Single(m => m.Name == "ToMaybe" && m.IsGenericMethodDefinition && m.GetGenericArguments()[0].IsValueType == wrappedType.IsValueType)
                .MakeGenericMethod(wrappedType)
                .Invoke(null, new[] { castValue });
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsConstructedGenericType && objectType.GetGenericTypeDefinition() == typeof(Maybe<>);
        }
    }
}