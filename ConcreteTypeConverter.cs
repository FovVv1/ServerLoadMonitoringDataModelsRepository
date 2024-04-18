using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ServerLoadMonitoringDataModels
{
    

    public class ConcreteTypeConverter<TConcrete> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            // Проверяем, является ли объект типом TConcrete или наследником от него
            return typeof(TConcrete).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // Десериализуем JSON в объект типа TConcrete
            return serializer.Deserialize(reader, typeof(TConcrete));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Сериализуем объект в JSON
            serializer.Serialize(writer, value);
        }
    }
}
