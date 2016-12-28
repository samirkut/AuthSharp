
using System;
using System.Composition;
using System.Text;

namespace AuthSharp.View
{
    public abstract class BaseView : IView
    {
        protected double currentProgress;

        [Import]
        public IDataAccess DataAccess { get; set; }

        public BaseView()
        {
            currentProgress = 0;
        }
        
        public abstract bool Login();

        public abstract void Home(bool forceRedraw, double progress);

        public abstract void Prefs();

        public abstract void New();

        public abstract void Delete();

        public virtual void Dispose()
        {

        }

        protected static string ReadPassword()
        {
            var sb = new StringBuilder();
            while (true)
            {
                var cki = Console.ReadKey(true);

                if (cki.Key == ConsoleKey.Escape)
                {
                    return string.Empty;
                }
                else if (cki.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0)
                        sb.Length--;
                }
                else if (cki.KeyChar == '\r' ||
                        cki.Key == ConsoleKey.Enter ||
                        (cki.Key == ConsoleKey.M && cki.Modifiers.HasFlag(ConsoleModifiers.Control)))
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