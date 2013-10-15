using System;

namespace Extensions {
    public static partial class StringExtensions
    {
        /// <summary>
        /// Deserialize the JSON string to an object.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T Deserialize<T>(this string data)
            where T : new()
        {
            return JsonFx.Json.JsonReader.Deserialize<T>(data);
        }
    }
}
