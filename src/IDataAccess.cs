using System.Collections.Generic;

namespace AuthSharp{
    public interface IDataAccess{
        bool Login(string password);

        void ChangePassword(string newPassword);

        IList<Entry> GetEntries();

        void AddUpdateEntry(Entry entry);

        void DeleteEntry(Entry entry);
    }
}