namespace Splunk
{
   using System;
   using System.Collections;
   using System.Collections.Generic;
   using System.Globalization;
   using System.Linq;

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
      /// Recursively selects dictionary elements
      /// </summary>
      /// <param name="output">The dictionary.</param>
      /// <param name="keys">The list of keys.</param>
      /// <returns>The output value from the dictionary</returns>
      public static object SelectRecursive(this IDictionary output, IEnumerable<string> keys = null)
      {
         if (keys == null)
         {
            return output;
         }
         else
         {
            var k = keys.GetEnumerator();
            object result = output;
            while ((result is IDictionary) && k.MoveNext())
            {
               result = ((IDictionary)result)[k.Current];
            }
            return result;
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
         // If we can't access the name, just give up right away
         if (!string.IsNullOrEmpty(data.Key))
         {
            string name = data.Key;

            var objects = data.Value as IDictionary<string, object>;
            if (objects != null)
            {
               return objects.ToString(formatString: name + "." + formatString);
            }

            string value = ToStringInvariant(data);

            if (keysForEmptyValues || !string.IsNullOrWhiteSpace(value))
            {
               return string.Format(formatString, name, value);
            }
         }
         return string.Empty;
      }

      /// <summary>
      /// Renders the object as a string using invariant round-trip friendly formats
      /// </summary>
      /// <param name="value">The value.</param>
      /// <returns>The string representation</returns>
      public static string ToStringInvariant(this object value)
      {
         string result = string.Empty;
         try
         {
            if (value != null)
            {
               // The "O" or "o" standard format specifier represents a custom date and time format string using a pattern that preserves time zone information.
               if (value is DateTime)
               {
                  result = ((DateTime) value).ToString("o");
               }
               else if (value is DateTimeOffset)
               {
                  result = ((DateTimeOffset) value).ToString("o");
               }
               else
               {
                  result = string.Format(CultureInfo.InvariantCulture, "{0}", value);
               }
            }
         }
         catch
         {
            result = string.Empty;
         }
         return result;
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
