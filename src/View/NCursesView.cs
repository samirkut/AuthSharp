using System;
using System.Text;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;
using AuthSharp.Native;
using AuthSharp.Model;
using System.Collections.Generic;
using System.Composition;
using AuthSharp.Util;

namespace AuthSharp.View
{
    public class NCursesView : ConsoleView
    {
        private readonly IntPtr stdscr;
        private int max_x, max_y;

        public NCursesView()
        {
            stdscr = NCurses.initscr();
            NCurses.start_color();
            NCurses.noecho();
            NCurses.cbreak();
            NCurses.keypad(stdscr, true);

            NCurses.curs_set(CursorVisibility.Invisible);

            NCurses.use_default_colors();
            NCurses.init_pair(1, NCurses.COLOR_GREEN, NCurses.COLOR_BLACK);
            NCurses.init_pair(2, NCurses.COLOR_YELLOW, NCurses.COLOR_BLACK);
            NCurses.init_pair(3, NCurses.COLOR_RED, NCurses.COLOR_BLACK);

            max_x = NCurses.getmaxx(stdscr);
            max_y = NCurses.getmaxy(stdscr);
        }

        public override bool Login()
        {
            if (DataAccess.RequireLogin())
            {
                DrawBorders(" AUTH 2FA ");
                NCurses.mvaddstr(1, 1, "Password ");
                NCurses.refresh();

                var pwd = ReadPassword();
                return DataAccess.Login(pwd);
            }

            return true;
        }

        public override void Delete()
        {
            int selected = 0;
            while (true)
            {
                DrawDelete(selected);
                NCurses.refresh();

                var cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.Escape)
                    break;

                var entries = DataAccess.GetEntries();
                if (cki.Key == ConsoleKey.DownArrow)
                {
                    selected++;
                    if (selected >= entries.Count) selected = 0;
                }
                else if (cki.Key == ConsoleKey.UpArrow)
                {
                    selected--;
                    if (selected < 0) selected = entries.Count - 1;
                }
                else if (cki.KeyChar == '\r' ||
                        cki.Key == ConsoleKey.Enter ||
                        (cki.Key == ConsoleKey.M && cki.Modifiers.HasFlag(ConsoleModifiers.Control)))
                {
                    DataAccess.DeleteEntry(entries[selected].Id);
                }
            }
        }

        public override void Home(bool forceRedraw, double progress)
        {
            if (progress < currentProgress)
                forceRedraw = true;

            currentProgress = Math.Min(progress, 1);
            if (!forceRedraw)
            {
                DrawProgress();
                NCurses.refresh();
                return;
            }

            DrawBorders(" AUTH 2FA ");

            //print entries
            var entries = DataAccess.GetEntries();

            var vspacing = Math.Max(1, (max_y - 3 - entries.Count) / (entries.Count + 1));
            var row = 1 + vspacing;
            var alt = false;
            foreach (var item in entries)
            {
                var gen = new TOTPGen(item.Secret);
                var name = item.Name;
                if (name.Length > 15)
                    name = name.Substring(0, 15);

                if (alt) NCurses.attron((int)NCurses.A_BOLD);
                NCurses.mvaddstr(row, (int)(max_x * 0.1), name);
                NCurses.mvaddstr(row, (int)(max_x * 0.35), gen.GetOTP());
                if (alt) NCurses.attroff((int)NCurses.A_BOLD);

                alt = !alt;
                row += vspacing + 1;
            }

            //print commands at the botton
            var cmds = new[] { "New (N)", "Del (D)", "Prefs (P)", "Exit (ESC)" };
            DrawCommandBar(cmds);

            //progress bar
            DrawProgress();

            NCurses.refresh();
        }


        public override void New()
        {
            DrawBorders(" NEW ");

            NCurses.attron((int)NCurses.A_BOLD);
            NCurses.mvaddstr(1, 1, "Name");
            NCurses.attroff((int)NCurses.A_BOLD);
            NCurses.refresh();
            var name = ReadInput(1, 15, 40);
            if (string.IsNullOrWhiteSpace(name))
            {
                ShowWarning(max_y - 2, 1, "Name cannot be blank.", true);
                return;
            }

            NCurses.attron((int)NCurses.A_BOLD);
            NCurses.mvaddstr(3, 1, "Account");
            NCurses.attroff((int)NCurses.A_BOLD);
            NCurses.refresh();
            var acct = ReadInput(3, 15, 40);

            NCurses.attron((int)NCurses.A_BOLD);
            NCurses.mvaddstr(5, 1, "Secret");
            NCurses.attroff((int)NCurses.A_BOLD);
            NCurses.refresh();
            var secret = ReadInput(5, 15, 40);
            if (string.IsNullOrWhiteSpace(secret))
            {
                ShowWarning(max_y - 2, 1, "Secret cannot be blank.", true);
                return;
            }

            var entry = new AccountEntry
            {
                Name = name,
                Account = acct,
                Secret = secret
            };
            DataAccess.AddUpdateEntry(entry);
            ShowInfo(max_y - 2, 1, "Entry added", true);
        }


        public override void Prefs()
        {
            DrawBorders(" PREFS ");
            NCurses.mvaddstr(1, 1, "New Password ");
            NCurses.refresh();
            var pwd1 = ReadPassword();
            if (string.IsNullOrWhiteSpace(pwd1)) return;

            NCurses.mvaddstr(2, 1, "Confirm New Password ");
            NCurses.refresh();
            var pwd2 = ReadPassword();

            if (pwd1 != pwd2)
            {
                ShowWarning(4, 1, "Sorry passwords don't match!", true);
            }
            else
            {
                DataAccess.ChangePassword(pwd1);
                ShowInfo(4, 1, "Password changed.", true);
            }
        }

        public override void Dispose()
        {
            NCurses.echo();
            NCurses.nocbreak();
            NCurses.keypad(stdscr, false);
            NCurses.endwin();
        }

        private void DrawDelete(int selected)
        {
            DrawBorders(" DELETE ");

            //print entries
            var entries = DataAccess.GetEntries();

            var vspacing = Math.Max(1, (max_y - 3 - entries.Count) / (entries.Count + 1));
            var row = 1 + vspacing;

            for (int i = 0; i < entries.Count; i++)
            {
                if (i == selected)
                {
                    NCurses.attron((int)NCurses.COLOR_PAIR(3));
                    NCurses.attron((int)NCurses.A_BOLD);
                }

                var name = entries[i].Name;
                if (name.Length > 15)
                    name = name.Substring(0, 15);
                var acct = entries[i].Account;

                NCurses.mvaddstr(row, 5, name);
                NCurses.mvaddstr(row, 25, acct);

                if (i == selected)
                {
                    NCurses.attroff((int)NCurses.COLOR_PAIR(3));
                    NCurses.attroff((int)NCurses.A_BOLD);
                }

                row += vspacing + 1;
            }

            //print commands at the botton
            var cmds = new[] { "Change Selection (Up/Down)", "Del (Enter)", "Cancel (ESC)" };
            DrawCommandBar(cmds);
        }

        private void DrawBorders(string heading)
        {
            NCurses.clear();

            NCurses.attron((int)NCurses.COLOR_PAIR(1));

            NCurses.border((char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0, (char)0);

            NCurses.attron((int)NCurses.A_BOLD);
            NCurses.mvaddstr(0, (max_x - heading.Length) / 2 - 1, heading);
            NCurses.attroff((int)NCurses.A_BOLD);

            NCurses.attroff((int)NCurses.COLOR_PAIR(1));
        }

        private void DrawCommandBar(string[] cmds)
        {
            //print commands at the bottom
            //asume commands dont exceed the win width for now
            var totalLen = cmds.Select(y => y.Length).Sum();
            var spacing = (max_x - 2 - totalLen) / (cmds.Length + 1); //numbr of spaces requred

            var col = spacing + 1;
            NCurses.attron((int)NCurses.COLOR_PAIR(2));
            foreach (var cmd in cmds)
            {
                NCurses.mvaddstr(max_y - 2, col, cmd);
                col += cmd.Length + spacing;
            }
            NCurses.attroff((int)NCurses.COLOR_PAIR(2));
        }

        private void DrawProgress()
        {
            var hsize = (int)((max_x - 2) * currentProgress);
            NCurses.move(max_y - 1, 1);
            NCurses.attron((int)NCurses.COLOR_PAIR(0));
            NCurses.hline((char)0, hsize);
            NCurses.attroff((int)NCurses.COLOR_PAIR(0));
        }

        private string ReadInput(int y, int x, int maxLength)
        {
            var ret = new StringBuilder();

            while (true)
            {
                var cki = Console.ReadKey(true);
                if (cki.Key == ConsoleKey.Escape)
                {
                    return string.Empty;
                }
                else if (cki.KeyChar == '\r' ||
                       cki.Key == ConsoleKey.Enter ||
                       (cki.Key == ConsoleKey.M && cki.Modifiers.HasFlag(ConsoleModifiers.Control)))
                {
                    return ret.ToString();
                }
                else if (cki.Key == ConsoleKey.Backspace)
                {
                    if (ret.Length > 0)
                    {
                        //delete the char from screen
                        NCurses.mvaddstr(y, x + ret.Length, " ");
                        NCurses.refresh();
                        ret.Length--;
                    }
                }
                else if (char.IsLetterOrDigit(cki.KeyChar) || char.IsPunctuation(cki.KeyChar) || char.IsSeparator(cki.KeyChar))
                {
                    if (ret.Length < maxLength)
                    {
                        //display on screen
                        NCurses.mvaddstr(y, x + ret.Length, cki.KeyChar.ToString());
                        NCurses.refresh();
                        ret.Append(cki.KeyChar);
                    }
                }
            }
        }

        private void ShowWarning(int y, int x, string msg, bool waitForKeyPress)
        {
            NCurses.attron((int)NCurses.COLOR_PAIR(3));
            NCurses.mvaddstr(y, x, msg);
            NCurses.attroff((int)NCurses.COLOR_PAIR(3));
            NCurses.refresh();

            if (waitForKeyPress)
                Console.ReadKey(true);
        }

        private void ShowInfo(int y, int x, string msg, bool waitForKeyPress)
        {
            NCurses.attron((int)NCurses.COLOR_PAIR(2));
            NCurses.mvaddstr(y, x, msg);
            NCurses.attroff((int)NCurses.COLOR_PAIR(2));
            NCurses.refresh();

            if (waitForKeyPress)
                Console.ReadKey(true);
        }
    }
}