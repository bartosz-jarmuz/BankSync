using System.IO;
using System.Xml.Linq;

namespace BankSync.Enrichers.Allegro.Tests
{
    internal static class Files
    {
        public static string Get(string name)
        {
            string dir = Directory.GetCurrentDirectory();
            dir = Path.Combine(dir, "Input");

            if (!Directory.Exists(dir))
            {
                throw new DirectoryNotFoundException($"Input folder does not exist: {dir}");
            }

            string filePath = Path.Combine(dir, name.TrimStart('\\'));

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Expected test file missing: {filePath}");
            }
            
            return filePath;
        }

        public static XDocument GetXml(string name)
        {
            string filePath = Get(name);
            
            return XDocument.Load(filePath);
        }
        
    }
}