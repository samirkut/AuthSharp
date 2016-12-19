using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AuthSharp.Util
{
    public class Encryptor
    {
        byte[] _key;

        public Encryptor(string password)
        {
            //compute sha1 hash of password and then use that as Key
            using (var sha = SHA512.Create())
            {
                _key = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public string Encrypt(string data)
        {
            using (var aes = Aes.Create())
            {
                aes.GenerateIV();
                aes.Key = _key;
                var encryptedBytes = Transform(aes.CreateEncryptor(), Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(encryptedBytes);
            }
        }

        public string Decrypt(string data)
        {
            using (var aes = Aes.Create())
            {
                aes.GenerateIV();
                aes.Key = _key;
                var decrypedBytest = Transform(aes.CreateDecryptor(), Convert.FromBase64String(data));
                return Encoding.UTF8.GetString(decrypedBytest);
            }
        }

        private byte[] Transform(ICryptoTransform transformer, byte[] data)
        {
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, transformer, CryptoStreamMode.Write))
                {
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(data);
                    }
                    return ms.ToArray();
                }
            }
        }
    }
}