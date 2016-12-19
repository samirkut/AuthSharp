using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Composition;
using Newtonsoft.Json;
using AuthSharp.Model;
using AuthSharp.Util;

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
            _prefsFilePath = Path.GetFullPath("~/.authSharpPrefs");
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

        public void DeleteEntry(AccountEntry entry)
        {
            var prefs = Load();
            throw new NotImplementedException();
        }

        public IList<AccountEntry> GetEntries()
        {
            var prefs = Load();
            return prefs?.Entries ?? new List<AccountEntry>();
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
            if (!File.Exists(_prefsFilePath))
                return new Prefs();

            var str = File.ReadAllText(_prefsFilePath);

            str = _encryptor.Decrypt(str);

            return JsonConvert.DeserializeObject<Prefs>(str);
        }

        private void Save(Prefs prefs)
        {
            var str = JsonConvert.SerializeObject(prefs);
            str = _encryptor.Encrypt(str);
            File.WriteAllText(_prefsFilePath, str);
        }

    }
}