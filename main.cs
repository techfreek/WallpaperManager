using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace WallpaperManager
{
    class main
    {
        static void main(string[] args)
        {
            List<string> dirs = null;
            string consoleInput = null;

            Console.WriteLine("Enter 'exit' at anytime to quit the current operation");

            if (parseSettings())
            {
                Console.WriteLine("Settings loaded");
                Console.Write("Enter new settings? (Y/N) ");
                consoleInput = Console.ReadLine();
            }

            if (consoleInput.ToLower().Contains("y"))
            {
                do
                {
                    Console.Write("Enter a directory (absolute path): ");
                    consoleInput = Console.ReadLine();
                    if ((!consoleInput.Contains("exit")) && (Directory.Exists(consoleInput)))
                    {
                        dirs.Add(consoleInput);
                    }
                } while (!consoleInput.Contains("exit"));

                directories = dirs;

                Console.Write("Scan recursively? (Y/N)");
                consoleInput = Console.ReadLine();

                if (consoleInput.ToLower().Contains("y"))
                {
                    recursive = true;
                }
                else
                {
                    recursive = false;
                }

                Console.Write("Update Interval? ");
                consoleInput = Console.ReadLine();

                updateInterval = System.Convert.ToInt32(consoleInput);

                saveSettings(recursive, dirs, updateInterval);
            }
        }

        static bool parseSettings()
        {
            string settingsPath = programFiles + doubleBackSlash + "settings.xml";
            if (File.Exists(settingsPath))
            {
                XDocument settings = XDocument.Load(settingsPath);

                recursive = System.Convert.ToBoolean(settings.Descendants("recursive").Single().Value);

                var tempDirs = from newDirs in settings.Descendants("directories")
                select new
                {
                    tempDirs = newDirs.Value
                };

                foreach (var tempDir in tempDirs)
                {
                    directories.Add(tempDir.ToString());
                }

                updateInterval = System.Convert.ToInt32(settings.Descendants("interval").Single().Value.ToString());
                return true;
            }
            return false;             
        }

        static void saveSettings(bool recursion, List<string> newDirectories, int newUpdateInterval)
        {
            XDocument settings = null;
            string settingsPath = programFiles + doubleBackSlash + "settings.xml";
            if(File.Exists(settingsPath))
            {
                settings = XDocument.Load(settingsPath);

                var root = settings.Root;

                root.Add(new XElement("recursive", recursive));

                root.Add(new XElement("directories"));

                var dirTag = root.Element("directories");

                foreach (string directory in newDirectories)
                {
                    dirTag.Add(new XElement("directory", directory));
                }

                root.Add(new XElement("interval", newUpdateInterval));
            }
            else
            {
                settings = new XDocument(new XElement("settings"));
                var root = settings.Root;

                root.Add(new XElement("recursive", recursive));

                root.Add(new XElement("directories"));

                var dirTag = root.Element("directories");

                foreach (string directory in newDirectories)
                {
                    dirTag.Add(new XElement("directory", directory));
                }

                root.Add(new XElement("interval", newUpdateInterval));
            }

            settings.Save(settingsPath);
            File.SetAttributes(settingsPath, FileAttributes.Hidden);
        }

        public static bool recursive { get; set; }
        public static List<string> directories { get; set; }
        public static int updateInterval { get; set; }
        public static string doubleBackSlash = "\\" + "\\";
        public static string programFiles = "C:" + doubleBackSlash + "Program Files (x86)" + doubleBackSlash + "Wallpaper Manager";
        
    }
}
