using System;
using System.Linq;
using System.Collections.Generic;
using System.Composition;
using System.Text;
using System.Threading;
using AuthSharp.Model;
using AuthSharp.Util;

namespace AuthSharp.View
{
    [Export(typeof(IView))]
    [Shared]
    public class ConsoleView : IView
    {
        [Import]
        public IDataAccess DataAccess { get; set; }

        public void Show()
        {
            if (DataAccess.RequireLogin() && !Login()) return;

            refreshMain();

            while (true)
            {
                var cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.Escape)
                    break;

                if (cki.Key == ConsoleKey.P)
                    showPrefs();
                if (cki.Key == ConsoleKey.N)
                    showNew();
                if (cki.Key == ConsoleKey.D)
                    showDelete();

                refreshMain();
            }
        }

        private bool Login()
        {
            Console.Write("Password: ");
            var pwd = ReadPassword();
            if (DataAccess.Login(pwd))
                return true;

            Console.WriteLine("Sorry. Password didnt work.");
            return false;
        }

        private void showDelete()
        {
            Console.CursorVisible = true;
            Console.WriteLine("Delete Entry");
            Console.WriteLine();

            var items = DataAccess.GetEntries().ToArray();
            for (var i = 0; i < items.Length; i++)
            {
                Console.WriteLine("{0}. {1}", i + 1, items[i].Name);
            }
            Console.WriteLine();
            Console.Write("Choose entry to delete (1-n): ");
            var str = Console.ReadLine();
            int n = 0;

            if (string.IsNullOrWhiteSpace(str)) return;

            if (!int.TryParse(str, out n) || n == 0 || n > items.Length)
            {
                Console.WriteLine("Invalid input");
            }
            else
            {
                //confirm once more to be safe
                Console.WriteLine("Deleting {0}. Press Y to confirm", items[n - 1].Name);
                var cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.Y)
                {
                    DataAccess.DeleteEntry(items[n - 1].Id);
                    Console.WriteLine("Deleted");
                }
            }

            Console.ReadKey(true);
        }

        private void showNew()
        {
            Console.CursorVisible = true;
            Console.WriteLine("Add New Entry");
            Console.WriteLine();

            Console.Write("Name: ");
            var name = Console.ReadLine();

            Console.Write("Acct: ");
            var acct = Console.ReadLine();

            Console.Write("Secret: ");
            var secret = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Name cannot be blank!");
            }
            else if (string.IsNullOrWhiteSpace(secret))
            {
                Console.WriteLine("Secret cannot be blank!");
            }
            else
            {
                var entry = new AccountEntry
                {
                    Name = name,
                    Account = acct,
                    Secret = secret
                };
                DataAccess.AddUpdateEntry(entry);
                Console.WriteLine("Added new entry.");
            }

            Console.ReadKey(true);
        }
        private void showPrefs()
        {
            Console.CursorVisible = true;
            Console.WriteLine("Change Password");
            Console.WriteLine();
            Console.Write("New Password: ");
            var newPwd1 = ReadPassword();

            Console.Write("Retype New Password: ");
            var newPwd2 = ReadPassword();

            if (newPwd1 != newPwd2)
            {
                Console.WriteLine("Sorry passwords dont match!");
            }
            else
            {
                DataAccess.ChangePassword(newPwd1);
                Console.WriteLine("Password changed successfully");
            }
            Console.ReadKey(true);
        }

        private void refreshMain()
        {
            Console.Clear();
            Console.CursorVisible = false;

            Console.WriteLine("Auth 2FA");
            Console.WriteLine();
            var items = DataAccess.GetEntries();
            foreach (var item in items)
            {
                var gen = new TOTPGen(item.Secret);
                var name = item.Name;
                if (name.Length > 15)
                    name = name.Substring(0, 15);
                Console.Write(name);
                Console.CursorLeft = 20;
                Console.WriteLine(gen.GetOTP());
            }
            Console.WriteLine();
            Console.WriteLine("Valid for {0} secs", TOTPGen.ValidSeconds);

            Console.WriteLine(); Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  :: New (N) :: Delete (D) :: Prefs (P) :: Exit (ESC) ::");
            Console.WriteLine(); Console.WriteLine();
            Console.ResetColor();
        }

        private static string ReadPassword()
        {
            var sb = new StringBuilder();
            while (true)
            {
                var cki = Console.ReadKey(true);

                if (cki.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0)
                        sb.Length--;
                }
                else if (cki.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return sb.ToString();
                }
                else
                {
                    sb.Append(cki.KeyChar);
                }
            }

        }
    }
}