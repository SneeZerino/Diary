using System.Collections.Generic;
using System.Linq;
using static Diary.Logic.Logger;

namespace Diary.Logic 
{
    public class ActiveDatabase 
    {
        public List<DiaryEntry> CurrentEntries { get; set; }
        public List<User> Udb { get; set; }
        public User CurrentUser { get; set; }
        public MySerializer EntrySaver { get; set; }
        public MySerializer UserSaver { get; set; }

        public ActiveDatabase() 
        {
            UserSaver = new MySerializer("Credentials");
            if (Native.DoesFileExist("Udb.dat")) {
                Udb = UserSaver.DeserializeFromFile<List<User>>("Udb");
            } else {
                Udb = new List<User>();
            }
        }

        // Erstellen eines neuen Benutzers

        public bool AddUser(User newUser) 
        {
            if (Udb.Exists(u => u.Username == newUser.Username)) 
            {
                Log("Tried to add existing user");
                return false;
            }
            newUser.UserID = Udb.Count > 0 ? Udb.Last().UserID + 1 : 0;
            Udb.Add(newUser);
            UserSaver.SerializeToFile(Udb, "Udb");
            CurrentUser = newUser;
            EntrySaver = new MySerializer(CurrentUser.Password);
            return true;
        }

        // Suchfunktion mit Query
 
        public List<DiaryEntry> Search(string query) 
        {
            var results = new List<DiaryEntry>();
            if (CurrentEntries == null || CurrentEntries.Count == 0) 
            {
                return results;
            }
            if (query == "*") 
            {
                return CurrentEntries;
            }
            var sQuery = query.Queryable();
            if (query.HasBorders("[", "]")) 
            {
                sQuery = query.Subset(1, 1).Queryable();
                return CurrentEntries.FindAll(e => e.Date.QuerySearch(sQuery));
            }
            foreach (var entry in CurrentEntries) {
                if (entry.Title.QuerySearch(sQuery) || entry.Body.QuerySearch(sQuery) || entry.Tags.QuerySearch(sQuery) || entry.Date.QuerySearch(sQuery)) 
                {
                    results.Add(entry);
                }
            }
            return results;
        }


        // Eintrag löschen mit ID -> wird im Suchfeld angezeigt

        public bool RemoveEntry(int queryId) 
        {
            if (!CurrentEntries.Exists(e => e.ID == queryId)) 
            {
                return false;
            }
            CurrentEntries.RemoveAll(e => e.ID == queryId);
            for (var i = 0; i < CurrentEntries.Count; i++) 
            {
                CurrentEntries[i].ID = i;
            }
            EntrySaver.SerializeToFile(CurrentEntries, $"Edb{CurrentUser.UserID}");
            return true;
        }


        // Eintrag hinzufügen

        public bool AddEntry(string title, string body, string tags, string PicURI) 
        {
            CurrentEntries ??= new List<DiaryEntry>();
            var cleanBody = body.RemoveWhiteSpace();
            if (CurrentEntries.Exists(e => e.Body.RemoveWhiteSpace() == cleanBody)) 
            {
                return false;
            }
            var newEntryId = CurrentEntries.Count > 0 ? CurrentEntries.Last().ID + 1 : 0;
            var newEntry = new DiaryEntry(title, body, newEntryId, tags, PicURI);
            CurrentEntries.Add(newEntry);
            EntrySaver.SerializeToFile(CurrentEntries, $"Edb{CurrentUser.UserID}");
            return true;
        }


        // Aktuellen Benutzer löschen

        public bool RemoveCurrentUser() 
        {
            if (CurrentUser == null) 
            {
                return false;
            }
            Udb.RemoveAll(u => u.UserID == CurrentUser.UserID);
            UserSaver.SerializeToFile(Udb, "Udb");
            Native.RemoveFile($"Edb{CurrentUser.UserID}");
            return true;
        }

        public bool Login(string username, string password) 
        {
            if (!Udb.Exists(u => u.Username == username)) 
            {
                return false;
            }
            var matchingUser = Udb.Find(u => u.Username == username);
            if (!Aes.IsValid(password, matchingUser.HashedPassword)) return false;
            CurrentUser = matchingUser;
            CurrentUser.Password = password;
            EntrySaver = new MySerializer(CurrentUser.Password);
            if (Native.DoesFileExist($"Edb{CurrentUser.UserID}.dat")) {
                CurrentEntries = EntrySaver.DeserializeFromFile<List<DiaryEntry>>($"Edb{CurrentUser.UserID}");
            } else 
            {
                CurrentEntries = new List<DiaryEntry>();
            }
            return true;
        }


        // Tagebucheinträge exportieren

        public bool ExportEntries() 
        {
            if (CurrentUser == null) 
            {
                return false;
            }
            EntrySaver.SerializeToXmlFile(CurrentEntries, $"Diary-Of-{CurrentUser.Username}");
            return true;
        }
    }
}
