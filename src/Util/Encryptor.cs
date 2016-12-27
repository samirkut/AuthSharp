using System;
using System.Linq;
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
            using (var sha = SHA256.Create())
            {
                _key = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public string Encrypt(string data)
        {
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.GenerateIV();
                    aes.Key = _key;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Mode = CipherMode.CBC;

                    var encryptedBytes = Encrypt(aes.CreateEncryptor(), data);
                    //prepend IV to the encryptedBytes
                    encryptedBytes = aes.IV.Concat(encryptedBytes).ToArray();
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string Decrypt(string data)
        {
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = _key;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Mode = CipherMode.CBC;

                    //we assume IV length is 16
                    var encryptedBytes = Convert.FromBase64String(data);
                    //read IV
                    aes.IV = encryptedBytes.Take(16).ToArray();
                    return Decrypt(aes.CreateDecryptor(), encryptedBytes.Skip(16).ToArray());
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private byte[] Encrypt(ICryptoTransform transformer, string data)
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

        private string Decrypt(ICryptoTransform transformer, byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var cs = new CryptoStream(ms, transformer, CryptoStreamMode.Read))
                {
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
}