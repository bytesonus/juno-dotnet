using System;
using System.Collections.Generic;
using System.Linq;
using JunoSDK.Models;
using Newtonsoft.Json.Linq;

namespace JunoSDK.Utils
{
    public static class Misc
    {
        public static Dictionary<string, MessageItem> ToMessageItemDictionary(this JObject jObject)
        {
            var dictionary = new Dictionary<string, MessageItem>();
            foreach (var key in jObject.Properties())
            {
                if (key.Type != JTokenType.String)
                {
                    return null;
                }
                var keyString = key.ToObject<string>();
                var item = jObject[keyString].ToMessageItem();

                dictionary.Add(keyString, item);
            }

            return dictionary;
        }
        
        public static List<MessageItem> ToMessageItemList(this JArray jArray)
        {
            return jArray.Select(arrayItem => arrayItem.ToMessageItem()).ToList();
        }

        public static MessageItem ToMessageItem(this JToken jToken)
        {
            switch (jToken.Type)
            {
                case JTokenType.Object:
                    return MessageItem.FromObject(jToken.ToObject<JObject>().ToMessageItemDictionary());
                case JTokenType.Array:
                    return MessageItem.FromObject(jToken.ToObject<JArray>().ToMessageItemList());
                case JTokenType.Integer:
                    return MessageItem.FromObject(jToken.ToObject<long>());
                case JTokenType.Float:
                    return MessageItem.FromObject(jToken.ToObject<double>());
                case JTokenType.String:
                    return MessageItem.FromObject(jToken.ToObject<string>());
                case JTokenType.Boolean:
                    return MessageItem.FromObject(jToken.ToObject<bool>());
                case JTokenType.Null:
                case JTokenType.Undefined:
                    return MessageItem.Empty;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public static bool IsUnix
        {
            get
            {
                var p = (int) Environment.OSVersion.Platform;
                return p == 4 || p == 6 || p == 128;
            }
        }
    }
}