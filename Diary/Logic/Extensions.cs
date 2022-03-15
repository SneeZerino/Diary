using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Diary.Logic {
    public static class Extensions {

        // Eine "Contains" Methodenerweiterung

        public static bool ContainsInsensitive(this string str, string toCheck) 
        {
            str = str.ToLower();
            toCheck = toCheck.ToLower();
            return str.Contains(toCheck);
        }

        // Entfernt alle Leerzeichen aus dem Text

        public static string RemoveWhiteSpace(this string str) 
        {
            return new string(str.ToCharArray()
        .Where(c => !char.IsWhiteSpace(c))
        .ToArray());
        }

        // Eine bequemere Art der Verwendung von Substring (Die benötigte Länge muss nicht berechnet werden)

        public static string Subset(this string str, int fromStart, int fromEnd) 
        {
            return str.Substring(fromStart, str.Length - fromEnd - fromStart);
        }

        // Überprüft, ob eine Zeichenfolge von 2 Zeichenfolgen begrenzt wird

        public static bool HasBorders(this string str, string startBorder, string endBorder) 
        {
            return str.StartsWith(startBorder) && str.EndsWith(endBorder);
        }

        // Suche nach eingegeben Wörtern

        public static bool QuerySearch(this string str, string[] words, int i = 0) 
        {
            if (words.Length == 0) 
            {
                return false;
            }
            if (i == words.Length - 1) 
            {
                return str.ContainsInsensitive(words[i]);
            }
            return str.ContainsInsensitive(words[i]) && str.QuerySearch(words, i + 1);
        }

        // teilt text auf mit entfernung con leeren Einträgen

        public static string[] ToWords(this string text) 
        {
            return text.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        }

        // Gibt text gesplittet aus, ausser es ist mit " " eingeschlossen

        public static string[] Queryable(this string text) 
        {
            List<string> parts = new List<string>();
            var first = text.IndexOf("\"", StringComparison.Ordinal);
            var last = text.LastIndexOf("\"", StringComparison.Ordinal);
            var doesContainExplicitSearch = first >= 0 && last > first;
            if (doesContainExplicitSearch) 
            {
                var length = last - first;
                var exp = text.Substring(first + 1, length - 1);
                text = text.Replace($"\"{exp}\"", " ");
                parts.Add(exp);
                parts.AddRange(text.ToWords());
            } else 
            {
                parts.AddRange(text.ToWords());
            }
            return parts.ToArray();
        }

        // Methode um das komplette Datum anzuzeigen

        public static string FullDateAndTime(this DateTime d) 
        {
            return $"{d.ToLongDateString()} at {d.ToLongTimeString()}";
        }


        // Titel Format = jedes Wort beginnt Gross

        public static string ToTitle(this string str) 
        {
            return new CultureInfo("en").TextInfo.ToTitleCase(str.ToLower());
        }
    }
}
