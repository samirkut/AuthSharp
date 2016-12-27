using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Composition;
using Newtonsoft.Json;
using AuthSharp.Model;
using AuthSharp.Util;
using System.Runtime.InteropServices;

namespace AuthSharp
{
    [Export(typeof(IDataAccess))]
    [Shared]
    public class DataAccess : IDataAccess
    {
        private readonly string _prefsFilePath;
        private Encryptor _encryptor;

        public DataAccess()
        {
            _encryptor = new Encryptor("0000");//default password?

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                _prefsFilePath = Environment.ExpandEnvironmentVariables("%USERPROFILE%/.authSharpConfig");
            else
                _prefsFilePath = Environment.ExpandEnvironmentVariables("%HOME%/.authSharpConfig");
        }

        public void AddUpdateEntry(AccountEntry entry)
        {
            var prefs = Load();
            if (string.IsNullOrEmpty(entry.Id))
                entry.Id = Guid.NewGuid().ToString();

            var existing = prefs.Entries?.FirstOrDefault(x => x.Id == entry.Id);
            if (existing != null)
                prefs.Entries?.Remove(existing);
            prefs.Entries.Add(entry);
            prefs.Entries = prefs.Entries.OrderBy(x => x.Name).ToList();
            Save(prefs);
        }

        public void DeleteEntry(string entryId)
        {
            var prefs = Load();
            var entry = prefs.Entries.FirstOrDefault(x => x.Id == entryId);
            if (entry != null)
                prefs.Entries.Remove(entry);
            Save(prefs);
        }

        public IList<AccountEntry> GetEntries()
        {
            var prefs = Load();
            return prefs?.Entries ?? new List<AccountEntry>();
        }

        public bool RequireLogin()
        {
            if (!File.Exists(_prefsFilePath))
                return false;

            return Load() == null;
        }
        public bool Login(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            _encryptor = new Encryptor(password);
            return Load() != null;
        }

        public void ChangePassword(string newPassword)
        {
            var prefs = Load();
            _encryptor = new Encryptor(newPassword);
            Save(prefs);
        }

        private Prefs Load()
        {
            try
            {
                if (!File.Exists(_prefsFilePath))
                    return new Prefs { Entries = new List<AccountEntry>() };

                var str = File.ReadAllText(_prefsFilePath);

                str = _encryptor.Decrypt(str);

                return JsonConvert.DeserializeObject<Prefs>(str);
            }
            catch
            {
                return null;
            }
        }

        private void Save(Prefs prefs)
        {
            var str = JsonConvert.SerializeObject(prefs);
            str = _encryptor.Encrypt(str);
            File.WriteAllText(_prefsFilePath, str);
        }

    }
}