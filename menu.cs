using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Windows;

namespace WallpaperManager
{
    public partial class menu : Form
    {
        public menu()
        {
            InitializeComponent();
            monitorArray = new List<monitorRow>();
            scanMonitors();
            this.totalScreens.Text = countMonitorArray().ToString();
            this.Show();
        }

        private void scanMonitors()
        {
            if (Screen.AllScreens.Count() != countMonitorArray())
            {
                if (countMonitorArray() != 0)
                { //Determine a more optimized method of updating the monitor set up
                    monitorArray.Clear();
                }
                foreach (var screen in Screen.AllScreens)
                {
                    Monitor temp = new Monitor();
                    // For each screen, add the screen properties to a list box.

                    temp.height = screen.WorkingArea.Height;
                    temp.width = screen.WorkingArea.Width;
                    temp.aspectRatio = Math.Round(((double)temp.width / temp.height), 2);

                    temp.xPos = screen.Bounds.X;
                    temp.yPos = screen.Bounds.Y;

                    temp.primary = screen.Primary;
                    insertMonitor(temp);
                }

            }
            
        }

        public int countMonitorArray()
        {
            int total = 0;
            foreach (monitorRow row in monitorArray)
            {
                total += row.row.Count;
            }
            return total;
        }

        private void insertMonitor(WallpaperManager.Monitor newMon)
        {
            bool inserted = false;
            if (monitorArray.Count == 0)
            {
                monitorRow newRow = new monitorRow();
                newRow.addMonitor(newMon);
                monitorArray.Add(newRow);
            }
            else
            {
                for (int i = 0; i < monitorArray.Count; i++)
                {
                    if (newMon.yPos == monitorArray[i].y_Cutoff)
                    { //In theory they should be on the same level if they are in the same row. I don't have
                        //enough monitors to test this theory at the moment.
                        monitorArray[i].addMonitor(newMon);
                        inserted = true;
                        break;
                    }
                }
                if (!inserted)
                {
                    monitorRow newRow = new monitorRow();
                    newRow.addMonitor(newMon);
                    monitorArray.Add(newRow);
                }
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox12_TextChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void menu_Load(object sender, EventArgs e)
        {
            
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {

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

        private List<monitorRow> monitorArray { get; set; }
        public static bool recursive { get; set; }
        public static bool battery { get; set; }
        public static List<string> directories { get; set; }
        public static int updateInterval { get; set; }

        //Constantly updated variables
        public static bool ACPower { get; set; }

        //Static variables that would be redundant to export
        public static string doubleBackSlash = "\\" + "\\";
        public static string userAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + doubleBackSlash + "Wallpaper Manager";
        public static string commonAppData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "Wallpaper Manager";

    }

    //Used when a user has vertically arranged monitors
    class monitorRow
    {
        public void addMonitor(Monitor newMonitor)
        {
            int width = 0, height = 0;

            if (row == null)
            {
                row = new List<Monitor>();
                y_Cutoff = newMonitor.yPos;
            }

            row.Add(newMonitor);

            foreach(Monitor mon in row)
            {
                width += mon.width;
                height += mon.height;
            }

            rowAspectRatio = Math.Round(((double)width / height), 2);
        }

        public List<Monitor> row { get; set; }
        public double rowAspectRatio { get; set; }
        public int y_Cutoff { get; set; }
    }

    //Holder for each individual monitor
    class Monitor
    {
        public int height { get; set; }
        public int width { get; set; }
        public int xPos { get; set; }
        public int yPos { get; set; }
        public bool primary { get; set; }
        public double aspectRatio { get; set; }
    }
}
