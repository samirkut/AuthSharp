using System;
using System.Composition;
using System.Composition.Hosting;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using AuthSharp.Util;
using AuthSharp.View;
using AuthSharp.Native;

namespace AuthSharp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IView view = null;
            try
            {
                view = new NCursesView();
            }
            catch
            {
                view = new ConsoleView();
            }

            //This does not work with NCurses. It seems that the init has to be the first line
            /*
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                view = new ConsoleView();
            else
                view = new NCursesView();
            */

            var configuration = new ContainerConfiguration()
                                    .WithAssembly(typeof(Program).GetTypeInfo().Assembly);

            using (var container = configuration.CreateContainer())
            {
                using (view)
                {
                    container.SatisfyImports(view);

                    if (view.Login())
                    {
                        var progress = 0D;
                        view.Home(true, progress);

                        while (true)
                        {
                            progress = (double)(TOTPGen.MaxValidSeconds - TOTPGen.ValidSeconds) / TOTPGen.MaxValidSeconds;

                            if (!Console.KeyAvailable)
                            {
                                view.Home(false, progress);
                                Thread.Sleep(200);
                            }
                            else
                            {
                                var cki = Console.ReadKey(true);
                                if (cki.Key == ConsoleKey.Escape)
                                    break;
                                else if (cki.Key == ConsoleKey.P)
                                    view.Prefs();
                                else if (cki.Key == ConsoleKey.N)
                                    view.New();
                                else if (cki.Key == ConsoleKey.D)
                                    view.Delete();

                                view.Home(true, progress);
                            }
                        }
                    }
                }
            }

        }
    }
}
