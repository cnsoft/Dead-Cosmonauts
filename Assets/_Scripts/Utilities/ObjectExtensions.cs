using System;
using System.Collections.Generic;
using System.Reflection;

namespace Extensions {
    public static partial class ObjectExtensions
    {
        private static Dictionary<Type, Dictionary<string, FieldInfo>> cachedFields = new Dictionary<Type, Dictionary<string, FieldInfo>>();

        private static Dictionary<string, FieldInfo> GetFieldDictionary<T>(this T obj){
            Dictionary <string, FieldInfo> dict = null;

            if (!cachedFields.TryGetValue(typeof(T),out dict)) {
                dict = new Dictionary<string, FieldInfo>();

                FieldInfo[] fields = typeof(T).GetFields();
                for (int i = 0; i < fields.Length; i++) {
                    FieldInfo info = fields [i];
                    dict [info.Name] = info;
                }
            }

            return dict;
        }

        /// <summary>
        /// Shallow updates the source object's fields to the destination object's fields.
        /// </summary>
        /// <returns>The destination object.</returns>
        /// <param name="source">Source object.</param>
        /// <param name="destionation">Destination object.</param>
        public static object ShallowUpdate<T>(this T source, T destination) {
            if (source == null || destination == null) {
                return null;
            }

            Dictionary<string, FieldInfo> sourceDict = source.GetFieldDictionary();
            Dictionary<string, FieldInfo> destDict = destination.GetFieldDictionary();
            foreach (string key in destDict.Keys) {
                if (sourceDict.ContainsKey(key)) {
                    object sourceValue = sourceDict[key].GetValue(source);
                    destDict[key].SetValue(destination,sourceValue);
                }
            }
            return destination;
        }

        /// <summary>
        /// Serialize the object to JSON.
        /// </summary>
        /// <param name="source">Source.</param>
        public static string Serialize(this object source)
        {
            return JsonFx.Json.JsonWriter.Serialize(source);
        }

        /// <summary>
        /// Clone the instance by serializing and deserializing it.
        /// </summary>
        /// <param name="source">Source.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T Clone<T>(this T source)
            where T : new()
        {
            return source.Serialize().Deserialize<T>();
        }
    }
}

