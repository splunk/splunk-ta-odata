namespace Splunk
{
   using System;
   using System.Collections;
   using System.Collections.Generic;
   using System.Globalization;
   using System.Linq;
   using System.Text;
   using System.Threading.Tasks;


   /// <summary>
   /// Helper extension methods for string dictionaries
   /// </summary>
   public static class StringDictionaryHelper
   {
      /// <summary>
      /// Converts a Dictionary to a filtered list of Name-Value pairs representing some or all of the entries in the dictionary
      /// </summary>
      /// <param name="output">The objects to be converted.</param>
      /// <param name="keys">An optional list of keys that we care about.</param>
      /// <returns>An enumerable collection of dynamic objects with Name and Value properties</returns>
      public static IEnumerable<KeyValuePair<string, object>> SelectKeyValuePairs(this IDictionary output, IEnumerable<string> keys = null)
      {
         if (keys == null)
         {
            foreach (DictionaryEntry kv in output)
            {
               yield return new KeyValuePair<string, object>(kv.Key.ToString(), kv.Value);
            }
         }
         else
         {
            foreach (var name in keys.Where(output.Contains))
            {
               yield return new KeyValuePair<string, object>(name, output[name]);
            }
         }
      }

      /// <summary>
      /// Selects from KeyValuePairs
      /// </summary>
      /// <param name="output">The output.</param>
      /// <param name="keys">The keys.</param>
      /// <returns>IEnumerable{KeyValuePair{System.StringSystem.Object}}.</returns>
      public static IEnumerable<KeyValuePair<string, object>> SelectKeyValuePairs(IEnumerable<KeyValuePair<string, object>> output, IEnumerable<string> keys)
      {
         return keys == null
             ? output.Select(kv => new KeyValuePair<string, object>(kv.Key, kv.Value))
             : output.Where(kv => keys.Contains(kv.Key)).Select(kv => new KeyValuePair<string, object>(kv.Key, kv.Value));
      }


      /// <summary>
      /// Returns a <see cref="System.String" /> that represents the KeyValuePairs
      /// </summary>
      /// <param name="data">The data.</param>
      /// <param name="includeEmpty">if set to <c>true</c> output KeyValuePairs without values.</param>
      /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
      public static string ToString(this IEnumerable<KeyValuePair<string, object>> data, string recordSeparator="\n", string formatString="{0}=\"{1}\"", bool includeEmpty = false)
      {
         return string.Join(recordSeparator, from kvp in data
                                             select kvp.ToString(formatString) into output
                                             where includeEmpty || !string.IsNullOrWhiteSpace(output)
                                             select output);
      }

      /// <summary>
      /// Returns a <see cref="System.String" /> that represents the KeyValuePairs
      /// </summary>
      /// <param name="data">The data.</param>
      /// <param name="includeEmpty">if set to <c>true</c> output KeyValuePairs without values.</param>
      /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
      public static string ToString(this IDictionary<string, object> data, string recordSeparator = "\n", string formatString = "{0}=\"{1}\"", bool includeEmpty = false)
      {
         return string.Join(recordSeparator, from kvp in data
                                             select kvp.ToString(formatString) into output
                                             where includeEmpty || !string.IsNullOrWhiteSpace(output)
                                             select output);
      }
      /// <summary>
      /// Converts objects with Names/Keys and Values into Name="Value" strings
      /// </summary>
      /// <param name="data">The key/value pair.</param>
      /// <param name="keysForEmptyValues">if set to <c>true</c> output 'Key=""' when the value is empty or whitespace.</param>
      /// <returns>The string representation of the object</returns>
      public static string ToString(this KeyValuePair<string, object> data, string formatString = "{0}=\"{1}\"", bool keysForEmptyValues = false)
      {
         var output = new StringBuilder();

         // If we can't access the name, just give up right away
         if (!string.IsNullOrEmpty(data.Key))
         {
            string value = string.Empty;

            string name = data.Key;

            try
            {
               if (data.Value != null)
               {
                  // The "O" or "o" standard format specifier represents a custom date and time format string using a pattern that preserves time zone information.
                  if (data.Value is DateTime)
                  {
                     value = ((DateTime)data.Value).ToString("o");
                  }
                  else if (data.Value is DateTimeOffset)
                  {
                     value = ((DateTimeOffset)data.Value).ToString("o");
                  }
                  else if (data.Value is IDictionary<string, object>)
                  {
                     return ((IDictionary<string, object>) data.Value).ToString(formatString: name + "." + formatString);
                  }
                  else
                  {
                     value = string.Format(CultureInfo.InvariantCulture, "{0}", data.Value);
                  }
               }
            }
            catch
            {
               value = string.Empty;
            }

            if (keysForEmptyValues || !string.IsNullOrWhiteSpace(value))
            {
               return string.Format(formatString, name, value);
            }
            else return string.Empty;
         }
         else return string.Empty;
      }


      /// <summary>
      /// Gets the value if the key exists, or returns the default value
      /// </summary>
      /// <typeparam name="K"></typeparam>
      /// <typeparam name="T"></typeparam>
      /// <param name="data">The Dictionary.</param>
      /// <param name="key">The key.</param>
      /// <param name="defaultValue">The default value.</param>
      /// <returns>The value (or the defaultValue)</returns>
      public static T GetValueOrDefault<K,T>(this IDictionary<K, T> data, K key, T defaultValue)
      {
         T value;
         return data.TryGetValue(key, out value) ? value : defaultValue;
      }
   }
}
