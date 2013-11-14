using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Drawing;
using System.Xml;
using System.Xml.Linq;

namespace WallpaperManager
{
    class Program
    {
        static void Main(string[] args)
        {
            string dir = null;
            int i = 0;

            double singeScreenRatio = 1.8;
            double doubleScreenRatio = 3.6;
            double tripleScreenRatio = 5.4;

            do
            {
                Console.Write("Enter a directory (absolute path): ");
                dir = Console.ReadLine();
            } while (!Directory.Exists(dir));

            var files = Directory.GetFiles(dir);

            List<imageNode> images = new List<imageNode>(files.Count() + 1);

            int height = 0;
            int width = 0;
            double ratio = 0;

            foreach (string img in files)
            {
                if (img.Contains(".png") || img.Contains(".jpg"))
                {
                    imageNode newImage = new imageNode();
                    Image currIMG = Image.FromFile(img);

                    height = currIMG.Height;
                    width = currIMG.Width;
                    ratio = (double)width / height;
                    ratio = Math.Round(ratio, 2);

                    newImage.filePath = img;
                    newImage.aspectRatio = ratio;
                    images.Insert(i, newImage);

                    currIMG.Dispose();
                    i++;

                }
            }

            Console.WriteLine(images.Count + " images indexed.");

            var single = from image in images
                         where image.aspectRatio <= singeScreenRatio
                         select image;

            var dbl = from image in images
                         where (image.aspectRatio > singeScreenRatio && image.aspectRatio <= doubleScreenRatio)
                         select image;

            var triple = from image in images
                         where (image.aspectRatio > doubleScreenRatio && image.aspectRatio <= tripleScreenRatio)
                         select image;

            Console.WriteLine("   " + single.Count() + " single monitor images.");
            Console.WriteLine("   " + dbl.Count() + " double monitor images.");
            Console.WriteLine("   " + triple.Count() + " triple monitor images.");


            xmlExport(images, dir);
        }


        public static bool xmlExport(List<imageNode> images, string dirPath)
        {
            XDocument lib = new XDocument(new XElement("wallpapers"));
            var root = lib.Root;

            foreach (imageNode image in images)
            {
                root.Add(new XElement("wallpaper",
                                new XAttribute("filepath", image.filePath),
                                new XAttribute("aspectRatio", image.aspectRatio)));
                
            }


            string libPath = dirPath + "\\" + "\\" + "wpmLib.xml";

            if (File.Exists(libPath))
                File.Delete(libPath);

            lib.Save(libPath);

            File.SetAttributes(libPath, FileAttributes.Hidden);
            return true;

        }
    }

    class imageNode
    {
        public string filePath { get; set; }
        public double aspectRatio { get; set; }
    }

}
