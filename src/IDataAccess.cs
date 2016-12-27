using System.Collections.Generic;
using AuthSharp.Model;

namespace AuthSharp{
    public interface IDataAccess
    {
        bool RequireLogin();

        bool Login(string password);

        void ChangePassword(string newPassword);

        IList<AccountEntry> GetEntries();

        void AddUpdateEntry(AccountEntry entry);

        void DeleteEntry(string entryId);
    }
}