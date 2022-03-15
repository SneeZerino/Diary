﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Diary.Logic 
{

    // Public encryption class to be used on strings or streams
    // Note: the key is not constant

    public class Aes 
    {
        private byte[] Key { get; set; }
        private readonly byte[] Vector = { 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 23, 19, 17 };

        private ICryptoTransform Encryptor { get; set; }
        private ICryptoTransform Decryptor { get; set; }
        private UTF8Encoding Encoder { get; set; }
        private RijndaelManaged Rm { get; set; }

        // Class initializer

        public Aes(string strKey) 
        {
            Key = CreateKey(strKey);
            Encoder = new UTF8Encoding();
            Rm = new RijndaelManaged {
                KeySize = 256,
            };
            Encryptor = Rm.CreateEncryptor(Key, Vector);
            Decryptor = Rm.CreateDecryptor(Key, Vector);
        }

        // Function that will hash and return a byte array from string

        private static byte[] CreateKey(string password) 
        {
            byte[] key = null;
            using (SHA256 shaM = new SHA256Managed()) 
            {
                key = shaM.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
            return key;
        }

        // Function used to encrypt original text

        public string Encrypt(string unencrypted) 
        {
            try { return Convert.ToBase64String(Transform(Encoder.GetBytes(unencrypted), Encryptor)); } catch (Exception e) { return $"Encryption failed: {e.Message}"; }
        }

        // Create an encrypted stream used to write files.

        public CryptoStream CreateEncryptionStream(Stream outputStream) 
        {
            return new CryptoStream(outputStream, Encryptor, CryptoStreamMode.Write);
        }

        // Function used to decipher encrypted text

        public string Decrypt(string encrypted) 
        {
            try { return Encoder.GetString(Transform(Convert.FromBase64String(encrypted), Decryptor)); } catch (Exception e) { return $"Decryption failed: {e.Message}"; }
        }

        // Returns a decryption stream used to read from encrypted files

        public CryptoStream CreateDecryptionStream(Stream inputStream) 
        {
            return new CryptoStream(inputStream, Decryptor, CryptoStreamMode.Read);
        }

        // Static function used to transform the input and encrypt it in individual bytes

        private static byte[] Transform(byte[] buffer, ICryptoTransform transform) 
        {
            try {
                var stream = new MemoryStream();
                using (var cs = new CryptoStream(stream, transform, CryptoStreamMode.Write)) {
                    cs.Write(buffer, 0, buffer.Length);
                }
                return stream.ToArray();
            } catch {
                return buffer;
            }
        }

        // Password Hashing Algorithm using Rfc2898DerivedBytes

        public static string GeneratePassword(string password, int iterations = 991) 
        {
            //generate a random salt for hashing
            var salt = new byte[24];
            new RNGCryptoServiceProvider().GetBytes(salt);

            //hash password given salt and iterations (default to 1000)
            //iterations provide difficulty when cracking
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
            byte[] hash = pbkdf2.GetBytes(24);

            //return delimited string with salt | #iterations | hash
            return Convert.ToBase64String(salt) + "|" + iterations + "|" +
                Convert.ToBase64String(hash);

        }

        // Validates password according to Hashed password from GeneratePassword()

        public static bool IsValid(string testPassword, string origDelimHash) 
        {
            //extract original values from delimited hash text
            var origHashedParts = origDelimHash.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            var origSalt = Convert.FromBase64String(origHashedParts[0]);
            var origIterations = int.Parse(origHashedParts[1]);
            var origHash = origHashedParts[2];

            //generate hash from test password and original salt and iterations
            var pbkdf2 = new Rfc2898DeriveBytes(testPassword, origSalt, origIterations);
            byte[] testHash = pbkdf2.GetBytes(24);
            

            //if hash values match then return success
            if (Convert.ToBase64String(testHash) == origHash) 
            {
                return true;
            }

            //no match return false
            return false;
        }
    }
}