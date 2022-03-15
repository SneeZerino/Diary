using System.IO; 
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization; 
using System.Security.Cryptography;

public class MySerializer {
    public string AesKey { get; private set; }
    private Diary.Logic.Aes AesProvider { get; set; }


    // Erstellt eine neue Klasse mit einem Key

    public MySerializer(string encryptionKey) 
    {
        AesKey = encryptionKey;
        AesProvider = new Diary.Logic.Aes(AesKey);
    }

    // Serialisierung

    public void SerializeToFile<T>(T data, string fileName, string path = "") 
    {
        using (var stream = File.Open($"{path + fileName}.dat", FileMode.Create, FileAccess.ReadWrite)) {
            using (CryptoStream cryptoStream = AesProvider.CreateEncryptionStream(stream)) {
                var bf = new BinaryFormatter();
                bf.Serialize(cryptoStream, data);
            }
        }
    }

    // De-Serialisierung

    public T DeserializeFromFile<T>(string fileName, string path = "") 
    {
        object data;
        using (var stream = File.Open($"{path + fileName}.dat", FileMode.Open, FileAccess.Read)) {
            using (CryptoStream cryptoStream = AesProvider.CreateDecryptionStream(stream)) {
                var bf = new BinaryFormatter();
                data = bf.Deserialize(cryptoStream);
            }
        }
        return (T)data;
    }

    // Serialisierung als .xml
    // Nur zum exportieren der Daten gedacht

    public void SerializeToXmlFile<T>(T data, string fileName, string path = "") 
    {
        using (TextWriter writer = new StreamWriter($"{path + fileName}.xml")) 
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(writer, data);
        }
    }
}