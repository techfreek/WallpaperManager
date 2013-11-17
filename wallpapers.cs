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
    class Wallpapers
    {
        static void indexer(List<string> directories)
        {
            List<List<imageNode>> parsedLibrary = null;
            List<bool> previouslyIndexed = null;

            int height = 0;
            int width = 0;
            double ratio = 0;

            double screenRatio = 0;

            string[] tempFiles = null;
            List<List<string>> files = null;

            for (int i = 0; i < directories.Count; i++)
            {
                tempFiles = Directory.GetFiles(directories[i]);

                if (files[i].Contains("wpmLib.xml"))
                    previouslyIndexed[i] = true;
                else
                    previouslyIndexed[i] = false;

                i++;
                files.Add(tempFiles.ToList());
            }



            for (int i = 0; i < directories.Count; i++)
            {
                imageLib[i] = new List<imageNode>(files[i].Count() + 1);
            }

            for (int i = 0; i < directories.Count(); i++)
            {
                if (previouslyIndexed.Count() == 0)
                {
                    parsedLibrary[i] = newLibrary(directories[i], i);
                }
                else
                {
                    parsedLibrary[i] = xmlParse(directories[i]);

                    var removedFiles = from wallpaper in parsedLibrary[i]
                                       where (!files[i].Contains(wallpaper.filePath))
                                       select wallpaper.filePath;

                    List<string> libraryFiles = new List<string>();

                    foreach (var wallpaper in parsedLibrary[i])
                    {
                        libraryFiles.Add(wallpaper.filePath);
                    }

                    var addedFiles = from file in files[i]
                                     where ((!libraryFiles.Contains(file)) && ((file.Contains(".png") || file.Contains(".jpg"))))
                                     select file;

                    if (addedFiles.Count() > 0)
                    {
                        List<string> aFiles = new List<string>();
                        foreach (var file in addedFiles)
                        {
                            aFiles.Add(file.ToString());
                        }
                        addToLib(aFiles, directories[i]);
                    }

                    if (removedFiles.Count() > 0)
                    {
                        List<string> rFiles = new List<string>();
                        foreach (var file in removedFiles)
                        {
                            rFiles.Add(file.ToString());
                        }
                        removeFromLib(rFiles, directories[i]);
                    }

                    libraryFiles.Clear();

                }
            }
        }
    
        public static List<imageNode> newLibrary(string dirPath, int i)
        {
            string[] files = Directory.GetFiles(dirPath);
            int height = 0,
                width = 0;
            double ratio = 0.0;
            imageLib[i] = new List<imageNode>(files[i].Count() + 1);

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
                    imageLib[i].Insert(imageLib.Count, newImage);
                    currIMG.Dispose();

                }
            }

            /*Console.WriteLine(images.Count + " images indexed.");

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
            Console.WriteLine("   " + triple.Count() + " triple monitor images.");*/


            xmlExport(imageLib[i], dirPath);
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
            }

            library.Save(filepath);
        }

        public List<imageNode> getImage(int screenWidth)
        {
            List<imageNode> images = null;
            double ratio = 1.8;  //calculations have shown the largest ratio is 1.8 between width and height.

            for (int i = 0; i < imageLib.Count; i++)
            {
                foreach(imageNode image in imageLib[i])
                {
                    if (((ratio * screenWidth) <= image.aspectRatio) && (image.aspectRatio < ((ratio + 1) * screenWidth)))
                    {
                        images.Add(image);
                    }
                }
            }
            return images;
        }

        public static List<string> libPath { get; set; }
        public static List<List<imageNode>> imageLib { get; set; }
    }

    

    class imageNode
    {
        public string filePath { get; set; }
        public double aspectRatio { get; set; }
    }

}