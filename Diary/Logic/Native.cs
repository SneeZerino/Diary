using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Threading;

namespace Diary.Logic 
{
    public static class Native 
    {
        public static void Invoke(Action action) => Dispatcher.CurrentDispatcher.Invoke(action);

        public static Version AssemblyVersion => Assembly.GetEntryAssembly()?.GetName().Version;

        // Löschfunktion

        public static bool RemoveFile(string fileName, string extension = ".dat") 
        {
            fileName += extension;
            if (!File.Exists(fileName)) 
            {
                return false;
            } else {
                try {
                    File.Delete(fileName);
                } catch (Exception e) 
                {
                    Logger.Log(e);
                    return false;
                }
                return true;
            }
        }

        // Exportieren in .txt

        public static void ListToFile(string filename, List<string> lst) 
        {
            try {
                using (TextWriter w = new StreamWriter(filename)) 
                {
                    foreach (var line in lst) 
                    {
                        w.WriteLine(line);
                    }
                }
            } catch (Exception e) 
            {
                e.Data.Add("DevMessage", "Exportieren fehlgeschlagen");
                Logger.Log(e);
            }
        }

        // Filesuche

        public static bool DoesFileExist(string filename) 
        {
            return File.Exists(filename);
        }
    }
}