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
            List<imageNode> parsedLibrary = null;
            bool previouslyIndexed = false;
            string dir = null;

            int i = 0;
            int height = 0;
            int width = 0;
            double ratio = 0;

            double singeScreenRatio = 1.8;
            double doubleScreenRatio = 3.6;
            double tripleScreenRatio = 5.4;

            do
            {
                Console.Write("Enter a directory (absolute path): ");
                dir = Console.ReadLine();
            } while (!Directory.Exists(dir));

            var files = Directory.GetFiles(dir);

            var library = from file in files
                          where file.Contains("wpmLib.xml")
                          select file;         

            List<imageNode> images = new List<imageNode>(files.Count() + 1);

            if (library.Count() == 1)
            {
                previouslyIndexed = true;
                Console.WriteLine("Library has already been indexed");
                parsedLibrary = xmlParse(dir);
            }

            if (previouslyIndexed)
            {
                var removedFiles = from wallpaper in parsedLibrary
                                   where (!files.Contains(wallpaper.filePath))
                                   select wallpaper.filePath;

                List<string> libraryFiles = new List<string>();

                foreach(var wallpaper in parsedLibrary)
                {
                    libraryFiles.Add(wallpaper.filePath);
                }

                var addedFiles = from file in files
                                 where ((!libraryFiles.Contains(file)) && ((file.Contains(".png") || file.Contains(".jpg"))))
                                 select file;

                if (addedFiles.Count() > 0)
                {
                    List<string> aFiles = new List<string>();
                    foreach (var file in addedFiles)
                    {
                        aFiles.Add(file.ToString());
                    }
                    addToLib(aFiles, dir);
                }
                
                if (removedFiles.Count() > 0)
                {
                    List<string> rFiles = new List<string>();
                    foreach(var file in removedFiles)
                    {
                        rFiles.Add(file.ToString());
                    }
                    removeFromLib(rFiles, dir);
                }

                

                libraryFiles.Clear();

            }
            

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

        public static List<imageNode> xmlParse(string dirPath)
        {
            XDocument library = XDocument.Load(dirPath + "\\" + "\\" + "wpmLib.xml");
            int i = 0;
            var wallpapers = from item in library.Descendants("wallpaper")
                            select new
                            {
                                filepath = item.Element("filepath"),
                                aspectRatio = item.Element("aspectRatio")
                            };
            List<imageNode> images = new List<imageNode>(wallpapers.Count());

            foreach (var wallpaper in wallpapers)
            {
                imageNode image = new imageNode();
                image.aspectRatio = Convert.ToDouble(wallpaper.aspectRatio.Value);
                image.filePath = wallpaper.filepath.Value.ToString();
                images.Insert(i, image);
                i++;
            }

            return images;
        }

        public static bool xmlExport(List<imageNode> images, string dirPath)
        {
            XDocument lib = new XDocument(new XElement("wallpapers"));
            var root = lib.Root;

            foreach (imageNode image in images)
            {
                root.Add(new XElement("wallpaper",
                                new XElement("filepath", image.filePath),
                                new XElement("aspectRatio", image.aspectRatio)));
                
            }


            string libPath = dirPath + "\\" + "\\" + "wpmLib.xml";

            if (File.Exists(libPath))
                File.Delete(libPath);

            lib.Save(libPath);

            File.SetAttributes(libPath, FileAttributes.Hidden);
            return true;

        }

        public static void addToLib(List<string> addedFiles, string dirPath)
        {
            string filepath = dirPath + "\\" + "\\" + "wpmLib.xml";

            XDocument library = XDocument.Load(filepath);

            var root = library.Root;

            foreach (string file in addedFiles)
            {
                Image currIMG = Image.FromFile(file);
                double ratio = (double)currIMG.Width / currIMG.Height;
                ratio = Math.Round(ratio, 2);
                currIMG.Dispose();

                root.Add(new XElement("wallpaper",
                                new XElement("filepath", file),
                                new XElement("aspectRatio", ratio)));
            }
            File.Delete(filepath);
            library.Save(filepath);
        }

        public static void removeFromLib(List<string> removedFiles, string dirPath)
        {
            string filepath = dirPath + "\\" + "\\" + "wpmLib.xml";

            XmlDocument library = new XmlDocument();
            library.Load(filepath);

            XmlElement root = library.DocumentElement;
            XmlNodeList wallpapers = root.ChildNodes;

            foreach (string file in removedFiles)
            {
                foreach (XmlNode node in wallpapers)
                {
                    //Console.WriteLine(node.FirstChild.ToString());
                    if (node.InnerText.Contains(file))
                    {
                        node.ParentNode.RemoveChild(node);

                    }
                }
                //string test = String.Format("//wallpaper[@filepath='{0}']", file);
                //XmlNode node = library.SelectSingleNode(test);
                
            }
            
            library.Save(filepath);
        }
    }

    

    class imageNode
    {
        public string filePath { get; set; }
        public double aspectRatio { get; set; }
    }

}
