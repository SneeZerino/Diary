using System;
using System.Collections;
using System.Collections.Generic;

namespace Diary.Logic 
{

    // loggt nur Errors

    public static class Logger 
    {
        private static List<string> Logged { get; } = new List<string>();
        public static bool HasErrorOccurred => ExceptionNumber > 0;
        private static int ExceptionNumber { get; set; } = 0;

        public static List<string> GetLog() => Logged;


        // Erstellt Nachricht für Log

        public static void Log(string message) 
        {
            ExceptionNumber++;
            Logged.Add(message);
        }

        // Loggt den kompletten Fehler

        public static void Log(Exception e) 
        {
            ExceptionNumber++;
            Logged.Add($"--->\tException {ExceptionNumber}\t<---");
            Logged.Add(e.Message);
            Logged.Add(e.Source);
            Logged.AddRange(e.Data.ToTable());
            Logged.Add($"--->\tEnd of exception\t<---");
        }

        // Zeigt den IDictionary der Exception.Data an

        private static List<string> ToTable(this IDictionary dict) 
        {
            var tbl = new List<string>();
            foreach (DictionaryEntry pair in dict) {
                tbl.Add(string.Format("{0, 0}: {1, 1}", pair.Key, pair.Value));
            }
            return tbl;
        }


        // Exportiert das Logfile

        public static void ExportLog() 
        {
            if (HasErrorOccurred) {
                Native.ListToFile("log.txt", Logged);
            }
        }
    }
}
