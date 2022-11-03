using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Dapper.Scaffolding
{
    class Program
    {
        public static string FrameworkProjectGuid { get; private set; }
        public static string ModelsProjectGuid { get; private set; }
        public static string RepositoriesProjectGuid { get; private set; }
        public static string ServicesProjectGuid { get; private set; }
        public static string DebugProjectGuid { get; private set; }
        public static string OutputFolder { get; private set; }
        public static string SolutionName { get; private set; }
        public static string NamespaceRoot { get; private set; }
        public static string ConnectionString { get; private set; }

        static Program()
        {
            FrameworkProjectGuid = Guid.NewGuid().ToString().ToUpper();
            ModelsProjectGuid = Guid.NewGuid().ToString().ToUpper();
            RepositoriesProjectGuid = Guid.NewGuid().ToString().ToUpper();
            ServicesProjectGuid = Guid.NewGuid().ToString().ToUpper();
            DebugProjectGuid = Guid.NewGuid().ToString().ToUpper();
            OutputFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output");
        }

        static void Main(string[] args)
        {
            var cla = new CommandLineApplication();
            {
                cla.Name = "Dapper Scaffolding Command Line Interface";
                cla.Description = "Generate scaffolding based on Dapper";
                cla.HelpOption("-? | -h | --help");
            }

            var sln = cla.Option("-sln | --solution <Solution>", "Solution Name (Mandatory)", CommandOptionType.SingleValue);
            var nr = cla.Option("-nr | --namespace-root <NamespaceRoot>", "Namespace Root (Default \"My\")", CommandOptionType.SingleValue);
            var conn = cla.Option("-conn | --connection-string <ConnectionString>", "Connection String (Mandatory)", CommandOptionType.SingleValue);

            cla.OnExecute(() =>
            {
                SolutionName = sln.Value();
                NamespaceRoot = nr.Value() ?? "My";
                ConnectionString = conn.Value();
                try
                {
                    Generate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                return 0;
            });

            cla.Execute(args);
        }

        private static void Generate()
        {
            XmlDocument xml = new XmlDocument();
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{typeof(Program).Namespace}.Templates.xml"))
            {
                xml.Load(stream);
                XmlNodeList datas = xml.GetElementsByTagName("data");
                foreach (XmlNode data in datas)
                {
                    var childNodes = data.ChildNodes.Cast<XmlNode>();
                    string value = childNodes.FirstOrDefault(x => x.Name == "value").InnerText;
                    string comment = childNodes.FirstOrDefault(x => x.Name == "comment").InnerText;
                    string fileContent = Translate(value);
                    string fileName = Translate(comment);
                    string filePath = Path.Combine(OutputFolder, fileName);
                    string folder = Path.GetDirectoryName(filePath);
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    Console.WriteLine(fileName);
                    File.WriteAllText(filePath, fileContent);
                }
            }
            Console.WriteLine("Done.");
            System.Diagnostics.Process.Start("explorer.exe", OutputFolder);
        }

        static string Translate(string input)
        {
            return input
                .Replace("${SLN}$", SolutionName)
                .Replace("${NR}$", NamespaceRoot)
                .Replace("${CONN}$", ConnectionString)
                .Replace("${FrameworkProjectGuid}$", FrameworkProjectGuid)
                .Replace("${ModelsProjectGuid}$", ModelsProjectGuid)
                .Replace("${RepositoriesProjectGuid}$", RepositoriesProjectGuid)
                .Replace("${ServicesProjectGuid}$", ServicesProjectGuid)
                .Replace("${DebugProjectGuid}$", DebugProjectGuid)
                ;
        }
    }
}
