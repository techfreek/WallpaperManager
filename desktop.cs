using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Diagnostics;


namespace WallpaperManager
{
    public partial class desktop : Form
    {
        public desktop(string imagename)
        {
            filename = imagename;
            InitializeComponent();

            Size monitor = System.Windows.Forms.SystemInformation.PrimaryMonitorSize;
            int monWidth = monitor.Width;
            int monHeight = monitor.Height;

            this.BackgroundImageLayout = ImageLayout.Center;
            this.Width = monWidth;
            this.Height = monHeight;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.ShowInTaskbar = false;
            
        }
        public void changeImage()
        {
            Image wallpaper = Image.FromFile(filename);
            
            this.BackgroundImage = wallpaper;
            //this.Enabled = false;
            this.WindowState = FormWindowState.Maximized;
        }

        public void start()
        {
            this.Show();
            this.SendToBack();
            this.Refresh(); 
        }


        public string filename { get; set; }
        public Thread picture = null;
    }
}
