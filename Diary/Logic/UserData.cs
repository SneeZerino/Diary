using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Diary.Logic {
    [Serializable()]
    public class UserData : ISerializable
    {
        public int UserID { get; set; }
        public List<DiaryEntry> UserEntries { get; set; }

        public UserData() { }

        // Serialization function

        public void GetObjectData(SerializationInfo info, StreamingContext context) 
        {
            info.AddValue("UserID", UserID);
            info.AddValue("UserEntries", UserEntries);
        }

        // Deserialization function

        public UserData(SerializationInfo info, StreamingContext ctxt) 
        {
            UserID = (int)info.GetValue("UserID", typeof(int));
            UserEntries = (List<DiaryEntry>)info.GetValue("UserEntries", typeof(List<DiaryEntry>));
        }
    }
}
