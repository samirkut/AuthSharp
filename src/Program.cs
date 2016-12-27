using System;
using System.Composition;
using System.Composition.Hosting;
using System.Reflection;
using AuthSharp.View;

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
                IView view = new ConsoleView();
                container.SatisfyImports(view);
                view.Show();
            }

        }
    }
}
