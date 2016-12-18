using System;
using System.Composition;
using System.Composition.Hosting;
using System.Reflection;

namespace AuthSharp
{
    public class Program
    {
        [Import]
        public IDataAccess DataAccess { get; set; }

        public static void Main(string[] args)
        {
            var configuration = new ContainerConfiguration()
                                    .WithAssembly(typeof(Program).GetTypeInfo().Assembly);

            using (var container = configuration.CreateContainer())
            {
                var propg = new Program();
                container.SatisfyImports(propg);

                Console.ReadLine();
            }
        }
    }
}
