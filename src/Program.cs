using System;
using System.Composition;
using System.Composition.Hosting;
using System.Reflection;
using DevZH.UI;

namespace AuthSharp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ContainerConfiguration()
                                    .WithAssembly(typeof(Program).GetTypeInfo().Assembly);

            using (var container = configuration.CreateContainer())
            {

                var app = new Application();

                var window = new MainWindow();
                container.SatisfyImports(window);

                app.Run(window);

                Console.ReadLine();
            }
        }
    }
}
