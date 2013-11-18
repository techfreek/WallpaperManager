using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Forms;

namespace WallpaperManager
{
    class manager
    {
        static void Main(string[] args)
        {
            string consoleInput = null;
            bool existingSettings = false;
            Wallpapers library = null; 

            Console.WriteLine("Enter 'exit' at anytime to quit the current operation");

            //Counts how many monitors are currently connected (this is updated live, so I can use this to rescan later, some other methods are constant...)
            numScreens = System.Windows.Forms.SystemInformation.MonitorCount;

            Console.WriteLine("You have " + numScreens + " screens");

            //Attempts to parse settings. If it is succesful, the app has been configured before
            if (parseSettings())
            {
                existingSettings = true;
                Console.WriteLine("Settings loaded");
                Console.Write("Enter new settings? (Y/N) ");
                consoleInput = Console.ReadLine();
            }

            //If it hasn't been run before, or the user wants to enter new settings
            if (!existingSettings || consoleInput.ToLower().Contains("y"))
            {
                changeSettings();
            }
            
            //pass the directories and opinion on recursion to the helper class
            library = new Wallpapers(directories, recursive);

            library.indexer();

            libraryStats(library);

        }

        static void libraryStats(Wallpapers library)
        {
            List<string> singleMonitor = library.getImageOfWidth(1);
            List<string> doubleMonitor = library.getImageOfWidth(2);
            List<string> tripleMonitor = library.getImageOfWidth(3);

            Console.WriteLine("Single Monitor Wallpapers: " + singleMonitor.Count);
            Console.WriteLine("Double Monitor Wallpapers: " + doubleMonitor.Count);
            Console.WriteLine("Triple Monitor Wallpapers: " + tripleMonitor.Count);
        }

        static void changeSettings()
        {
            List<string> dirs = new List<string>();
            string consoleInput = null;

            do
            {
                Console.Write("Enter a directory (absolute path): ");
                consoleInput = Console.ReadLine();
                if ((!consoleInput.Contains("exit")) && (Directory.Exists(consoleInput)) && (!dirs.Contains(consoleInput)))
                { //makes sure we don't enter an exit command, an already existing directory, or a directory that is not on the drive
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

            //Idea borrowed from windows where it can disable on battery
            Console.Write("Run on battery power? (Y/N)");
            consoleInput = Console.ReadLine();

            if (consoleInput.ToLower().Contains("y"))
            {
                battery = true;
            }
            else
            {
                battery = false;
            }

            Console.Write("Update Interval (minutes)? ");
            consoleInput = Console.ReadLine();

            updateInterval = System.Convert.ToInt32(consoleInput);

            //passes settings on so they can be saved for future use
            saveSettings();
        }

        static bool parseSettings()
        {
            string settingsPath = userAppData + doubleBackSlash + "settings.xml";
            if (File.Exists(settingsPath))
            {
                XDocument settings = XDocument.Load(settingsPath);
                directories = new List<string>();

                //These can have only 1 value, so they are handled in 1 line. Uses System.Convert._____ to get them from strings to the appropriate datatypes
                recursive = System.Convert.ToBoolean(settings.Descendants("recursive").Single().Value);
                updateInterval = System.Convert.ToInt32(settings.Descendants("interval").Single().Value.ToString());
                battery = System.Convert.ToBoolean(settings.Descendants("runOnBattery").Single().Value);

                //There can be multiple "directory" elements, so they haved to be saved (according to the query syntax) as an 'a variable. I later will need to convert it to a known datatype
                var tempDirs = from newDir in settings.Descendants("directory")
                select new
                {
                    dir = newDir.Value
                };

                foreach (var tempDir in tempDirs)
                {
                    directories.Add(tempDir.dir.ToString());
                }

                
                return true;
            }
            //If settings don't exist, then the app has not been run before
            return false;             
        }

        static void saveSettings()
        {
            XDocument settings = null;
            string settingsPath = userAppData + doubleBackSlash + "settings.xml";
            if(File.Exists(settingsPath))
            { //Currently the algorithm re-writes the file with the new information.
                settings = XDocument.Load(settingsPath);
                settings.Root.RemoveAll();
            }
            else
            { //If the file doesn't exist, it has to be recreated
                settings = new XDocument(new XElement("settings"));
            }

            var root = settings.Root;
               
            root.Add(new XElement("recursive", recursive));
            root.Add(new XElement("runOnBattery", battery));
            root.Add(new XElement("interval", updateInterval));

            root.Add(new XElement("directories"));
            var dirTag = root.Element("directories");
            foreach (string directory in directories)
            {
                dirTag.Add(new XElement("directory", directory));
            }

            if (!Directory.Exists(userAppData))
            { //Verifies that the application folder exists before attempting to save
                Directory.CreateDirectory(userAppData);
            }

            settings.Save(settingsPath);

        }

        //Saved variables
        public static bool recursive { get; set; }
        public static bool battery { get; set; }
        public static List<string> directories { get; set; }
        public static int updateInterval { get; set; }

        //Constantly updated variables
        public static bool ACPower { get; set; }
        public static int numScreens { get; set; }

        //Static variables that would be redundant to export
        public static string doubleBackSlash = "\\" + "\\";
        public static string userAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + doubleBackSlash + "Wallpaper Manager";
        public static string commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "Wallpaper Manager";
        
    }
}
