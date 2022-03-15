using System;
using System.Runtime.Serialization;

namespace Diary.Logic 
{
    [Serializable()]
    public class DiaryEntry : ISerializable 
    {
        public int ID { get; set; }
        public string Date { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Tags { get; set; }
        public string PictureURI { get; set; }

        public DiaryEntry() { }

        // Initialization funktion

        public DiaryEntry(string title, string body, int entryId, string tags, string PicURI) 
        {
            Title = title.ToTitle();
            Body = body;
            Date = DateTime.Now.FullDateAndTime();
            ID = entryId;
            Tags = tags.ToTitle();
            PictureURI = PicURI;
        }

        // Serialization funktion

        public void GetObjectData(SerializationInfo info, StreamingContext context) 
        {
            info.AddValue("ID", ID);
            info.AddValue("Date", Date);
            info.AddValue("Title", Title);
            info.AddValue("Body", Body);
            info.AddValue("Tags", Tags);
            info.AddValue("PictureURI", PictureURI);
        }

        // De-serialization funktion

        public DiaryEntry(SerializationInfo info, StreamingContext ctxt) 
        {
            ID = (int)info.GetValue("ID", typeof(int));
            Date = (string)info.GetValue("Date", typeof(string));
            Title = (string)info.GetValue("Title", typeof(string));
            Body = (string)info.GetValue("Body", typeof(string));
            Tags = (string)info.GetValue("Tags", typeof(string));
            PictureURI = (string)info.GetValue("PictureURI", typeof(string));
        }
    }
}
