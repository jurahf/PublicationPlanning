using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PublicationPlanning.StoredModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PublicationPlanning.Repositories
{
    public class EntityJsonConverter : JsonConverter
    {
        public EntityJsonConverter()
        {
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value);

            if (t.Type != JTokenType.Object)
            {
                t.WriteTo(writer);
            }
            else
            {
                JObject o = (JObject)t;

                foreach (var prop in o.Properties())
                {
                    PropertyInfo info = value.GetType().GetProperty(prop.Name);
                    // если у сущности есть связи с другими сущностями
                    if (info.PropertyType.GetInterface(nameof(IStoredEntity)) != null)
                    {
                        if (prop.Value == null || prop.Value.Type == JTokenType.Null)
                            continue;

                        // то сериализуем только связанные ID, а не целиком связанные сущности
                        JToken propToken = prop.Value;
                        JObject propObject = (JObject)propToken;

                        (var fieldName, var fieldValue) = propObject.Properties()
                            .Where(x => x.Name == nameof(IStoredEntity.Id))
                            .Select(x => (x.Name, x.Value))
                            .First();

                        propObject.RemoveAll();
                        propObject.Add(fieldName, fieldValue);
                    }

                    // TODO: а что со списками сущностей?
                    if (info.PropertyType.IsArray)
                        info.PropertyType.GetElementType();
                }

                o.WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter."); ;
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanConvert(Type objectType)
        {
            //return objectType.IsInstanceOfType(typeof(IStoredEntity)); // так почему-то не работает
            return objectType.GetInterface(nameof(IStoredEntity)) != null;
        }
    }
}
